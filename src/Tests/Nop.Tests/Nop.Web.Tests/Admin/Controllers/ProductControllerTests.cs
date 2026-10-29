using AwesomeAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using NUnit.Framework;

namespace Nop.Tests.Nop.Web.Tests.Admin.Controllers;

[TestFixture]
public class ProductControllerTests : BaseNopTest
{
    private Mock<IWorkContext> _workContext;
    private Mock<IProductTagService> _productTagService;
    private Mock<IProductModelFactory> _productModelFactory;
    private Mock<IPermissionService> _permissionService;
    private ProductController _controller;

    [SetUp]
    public void SetUp()
    {
        _workContext = new Mock<IWorkContext>(MockBehavior.Strict);
        _productTagService = new Mock<IProductTagService>(MockBehavior.Strict);
        _productModelFactory = new Mock<IProductModelFactory>(MockBehavior.Strict);
        _permissionService = new Mock<IPermissionService>(MockBehavior.Strict);

        _workContext.Setup(x => x.GetCurrentVendorAsync())
            .ReturnsAsync((Vendor)null);

        _controller = CreateController();
    }

    [Test]
    public async Task BulkProductTags_ValidTag_ReturnsView()
    {
        var tag = new ProductTag { Id = 42, Name = "Summer Sale" };
        _productTagService.Setup(x => x.GetProductTagByIdAsync(tag.Id))
            .ReturnsAsync(tag);
        _permissionService.Setup(x => x.AuthorizeAsync(
                StandardPermission.Catalog.PRODUCT_TAGS_CREATE_EDIT_DELETE))
            .ReturnsAsync(true);

        var expected = new BulkProductTagSearchModel
        {
            ProductTagId = tag.Id,
            ProductTagName = tag.Name,
            CanEditTags = true
        };
        _productModelFactory
            .Setup(x => x.PrepareBulkProductTagSearchModelAsync(
                It.IsAny<BulkProductTagSearchModel>(), tag, true))
            .ReturnsAsync(expected);

        var result = await _controller.BulkProductTags(tag.Id);

        var view = result.Should().BeOfType<ViewResult>().Which;
        view.Model.Should().BeSameAs(expected);
        _productModelFactory.Verify(x => x.PrepareBulkProductTagSearchModelAsync(
            It.IsAny<BulkProductTagSearchModel>(), tag, true), Times.Once);
    }

    [Test]
    public async Task BulkProductTags_InvalidTag_ReturnsNotFound()
    {
        _productTagService.Setup(x => x.GetProductTagByIdAsync(999999))
            .ReturnsAsync((ProductTag)null);

        var result = await _controller.BulkProductTags(999999);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Test]
    public async Task BulkProductTagList_ValidSearch_ReturnsJson()
    {
        var search = new BulkProductTagSearchModel
        {
            ProductTagId = 42,
            SearchProductName = "Laptop",
            Start = 0,
            Length = 10
        };
        var expected = new BulkProductTagListModel
        {
            Data = new List<ProductTagProductModel>
            {
                new() { Id = 7, Name = "Laptop", IsTagged = true }
            },
            RecordsFiltered = 1,
            RecordsTotal = 1
        };
        _productTagService.Setup(x => x.GetProductTagByIdAsync(search.ProductTagId))
            .ReturnsAsync(new ProductTag { Id = search.ProductTagId, Name = "Sale" });
        _productModelFactory.Setup(x => x.PrepareBulkProductTagListModelAsync(search))
            .ReturnsAsync(expected);

        var result = await _controller.BulkProductTagList(search);

        var json = result.Should().BeOfType<JsonResult>().Which;
        json.Value.Should().BeSameAs(expected);
        _productModelFactory.Verify(
            x => x.PrepareBulkProductTagListModelAsync(search), Times.Once);
    }

    [Test]
    public async Task BulkProductTagList_InvalidPaging_ReturnsBadRequest()
    {
        var searches = new[]
        {
            new BulkProductTagSearchModel { ProductTagId = 42, Start = 0, Length = 0 },
            new BulkProductTagSearchModel { ProductTagId = 42, Start = -1, Length = 10 }
        };

        foreach (var search in searches)
        {
            var result = await _controller.BulkProductTagList(search);
            result.Should().BeOfType<BadRequestObjectResult>();
        }

        _productTagService.Verify(
            x => x.GetProductTagByIdAsync(It.IsAny<int>()), Times.Never);
        _productModelFactory.Verify(
            x => x.PrepareBulkProductTagListModelAsync(
                It.IsAny<BulkProductTagSearchModel>()), Times.Never);
    }

    [Test]
    public async Task SaveBulkProductTags_ValidChanges_ReturnsSuccess()
    {
        const int tagId = 42;
        var addIds = new[] { 7, 8 };
        var removeIds = new[] { 9 };
        _productTagService.Setup(x => x.UpdateBulkProductTagAssignmentsAsync(
                tagId, addIds, removeIds))
            .ReturnsAsync(3);

        var result = await _controller.SaveBulkProductTags(
            tagId, addIds, removeIds);

        var json = result.Should().BeOfType<JsonResult>().Which;
        var data = JObject.FromObject(json.Value);
        data.Value<bool>("success").Should().BeTrue();
        data.Value<int>("updatedCount").Should().Be(3);
        _productTagService.Verify(x => x.UpdateBulkProductTagAssignmentsAsync(
            tagId, addIds, removeIds), Times.Once);
    }

    [Test]
    public async Task SaveBulkProductTags_InvalidProductIds_ReturnsBadRequest()
    {
        const int tagId = 42;
        var invalidIds = new[] { -1 };
        var removeIds = Array.Empty<int>();
        _productTagService.Setup(x => x.UpdateBulkProductTagAssignmentsAsync(
                tagId, invalidIds, removeIds))
            .ThrowsAsync(new ArgumentException("Product identifiers must be positive."));

        var result = await _controller.SaveBulkProductTags(
            tagId, invalidIds, removeIds);

        var badRequest = result.Should().BeOfType<BadRequestObjectResult>().Which;
        badRequest.Value.Should().Be("Product identifiers must be positive.");
    }

    [Test]
    public async Task BulkProductTags_Vendor_ReturnsForbid()
    {
        _workContext.Setup(x => x.GetCurrentVendorAsync())
            .ReturnsAsync(new Vendor { Id = 12, Name = "Vendor" });

        var result = await _controller.BulkProductTags(42);

        result.Should().BeOfType<ForbidResult>();
        _productTagService.Verify(
            x => x.GetProductTagByIdAsync(It.IsAny<int>()), Times.Never);
    }

    private ProductController CreateController()
    {
        // These actions use only the four dependencies supplied below.
        // Other dependencies are not needed for this isolated controller test.
        return new ProductController(
            adminAreaSettings: null,
            customerSettings: null,
            aclService: null,
            artificialIntelligenceService: null,
            backInStockSubscriptionService: null,
            baseAdminModelFactory: null,
            categoryService: null,
            copyProductService: null,
            currencyService: null,
            customerActivityService: null,
            discountService: null,
            downloadService: null,
            eventPublisher: null,
            exportManager: null,
            filterLevelValueModelFactory: null,
            filterLevelValueService: null,
            httpClientFactory: null,
            importManager: null,
            languageService: null,
            localizationService: null,
            localizedEntityService: null,
            manufacturerService: null,
            fileProvider: null,
            notificationService: null,
            pdfService: null,
            permissionService: _permissionService.Object,
            pictureService: null,
            priceListService: null,
            productAttributeFormatter: null,
            productAttributeParser: null,
            productAttributeService: null,
            productModelFactory: _productModelFactory.Object,
            productService: null,
            productTagService: _productTagService.Object,
            settingService: null,
            shoppingCartService: null,
            specificationAttributeService: null,
            storeContext: null,
            storeMappingService: null,
            translationModelFactory: null,
            urlRecordService: null,
            videoService: null,
            warehouseService: null,
            webHelper: null,
            workContext: _workContext.Object,
            currencySettings: null,
            localizationSettings: null,
            mediaSettings: null,
            taxSettings: null,
            vendorSettings: null);
    }
}
