
using AwesomeAssertions;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Media;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using NUnit.Framework;

namespace Nop.Tests.Nop.Web.Tests.Admin.Factories;

[TestFixture]
public class ProductModelFactoryTests : WebTest
{
    private IProductModelFactory _factory;
    private IProductService _productService;
    private IPictureService _pictureService;
    private IRepository<ProductTag> _tagRepository;
    private IRepository<ProductProductTagMapping> _mappingRepository;
    private IStaticCacheManager _cacheManager;

    private ProductTag _tag;
    private ProductProductTagMapping _mapping;
    private Product _taggedProduct;
    private Product _untaggedProduct;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        _factory = GetService<IProductModelFactory>();
        _productService = GetService<IProductService>();
        _pictureService = GetService<IPictureService>();
        _tagRepository = GetService<IRepository<ProductTag>>();
        _mappingRepository = GetService<IRepository<ProductProductTagMapping>>();
        _cacheManager = GetService<IStaticCacheManager>();

        var products = await _productService.SearchProductsAsync(
            showHidden: true,
            pageSize: 2);

        if (products.Count < 2)
            throw new AssertionException("At least two test products are required.");

        _taggedProduct = products[0];
        _untaggedProduct = products[1];

        // Create a dedicated test tag.
        _tag = new ProductTag
        {
            Name = $"BulkTagTest_{Guid.NewGuid():N}"
        };

        await _tagRepository.InsertAsync(_tag);

        // Assign the tag to only one product.
        _mapping = new ProductProductTagMapping
        {
            ProductId = _taggedProduct.Id,
            ProductTagId = _tag.Id
        };

        await _mappingRepository.InsertAsync(_mapping);

        await _cacheManager.RemoveByPrefixAsync(
            NopEntityCacheDefaults<ProductTag>.Prefix);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        if (_mapping?.Id > 0)
            await _mappingRepository.DeleteAsync(_mapping);

        if (_tag?.Id > 0)
            await _tagRepository.DeleteAsync(_tag);

        if (_cacheManager != null)
            await _cacheManager.RemoveByPrefixAsync(
                NopEntityCacheDefaults<ProductTag>.Prefix);
    }

    [Test]
    public async Task PrepareBulkProductTagList_MapsProductProperties()
    {
        var result = await PrepareProductAsync(_taggedProduct);

        result.Id.Should().Be(_taggedProduct.Id);
        result.Name.Should().Be(_taggedProduct.Name);
        result.Sku.Should().Be(_taggedProduct.Sku);
        result.Published.Should().Be(_taggedProduct.Published);

        var picture = (await _pictureService.GetPicturesByProductIdAsync(
            _taggedProduct.Id, 1)).FirstOrDefault();

        var (expectedUrl, _) = await _pictureService.GetPictureUrlAsync(
            picture, 75);

        result.PictureThumbnailUrl.Should().Be(expectedUrl);
    }

    [Test]
    public async Task PrepareBulkProductTagList_SetsIsTaggedCorrectly()
    {
        var tagged = await PrepareProductAsync(_taggedProduct);
        var untagged = await PrepareProductAsync(_untaggedProduct);

        tagged.IsTagged.Should().BeTrue();
        untagged.IsTagged.Should().BeFalse();
    }

    private async Task<ProductTagProductModel> PrepareProductAsync(
        Product product)
    {
        var searchModel = new BulkProductTagSearchModel
        {
            ProductTagId = _tag.Id,
            SearchProductName = product.Name,
            Start = 0,
            Length = 100
        };

        var result = await _factory.PrepareBulkProductTagListModelAsync(
            searchModel);

        return result.Data.Single(item => item.Id == product.Id);
    }
}
