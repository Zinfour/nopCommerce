using AwesomeAssertions;
using Nop.Core.Domain.Catalog;
using Nop.Data;
using Nop.Services.Catalog;
using NUnit.Framework;

namespace Nop.Tests.Nop.Services.Tests.Catalog;

[TestFixture]
public class ProductTagServiceTests : ServiceTest
{
    private IProductTagService _productTagService;
    private IRepository<Product> _productRepository;
    private IRepository<ProductTag> _tagRepository;
    private IRepository<ProductProductTagMapping> _mappingRepository;

    private readonly List<Product> _products = [];
    private ProductTag _tag;
    private ProductTag _otherTag;
    private string _namePrefix;

    [SetUp]
    public async Task SetUp()
    {
        _productTagService = GetService<IProductTagService>();
        _productRepository = GetService<IRepository<Product>>();
        _tagRepository = GetService<IRepository<ProductTag>>();
        _mappingRepository = GetService<IRepository<ProductProductTagMapping>>();

        _namePrefix = $"BulkTagTest_{Guid.NewGuid():N}";

        _tag = new ProductTag { Name = $"{_namePrefix}_Tag" };
        _otherTag = new ProductTag { Name = $"{_namePrefix}_OtherTag" };

        await _tagRepository.InsertAsync(_tag);
        await _tagRepository.InsertAsync(_otherTag);

        // Each test gets four isolated products in a predictable name order.
        foreach (var suffix in new[] { "A", "B", "C", "D" })
        {
            var product = new Product
            {
                Name = $"{_namePrefix}_{suffix}",
                Sku = $"{_namePrefix}_{suffix}",
                ProductType = ProductType.SimpleProduct,
                Published = true,
                Deleted = false,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow
            };

            await _productRepository.InsertAsync(product);
            _products.Add(product);
        }
    }

    [TearDown]
    public async Task TearDown()
    {
        // Remove mappings before their tags and products.
        if (_mappingRepository != null && (_tag?.Id > 0 || _otherTag?.Id > 0))
        {
            var tagId = _tag?.Id ?? 0;
            var otherTagId = _otherTag?.Id ?? 0;
            await _mappingRepository.DeleteAsync(mapping =>
                mapping.ProductTagId == tagId || mapping.ProductTagId == otherTagId);
        }

        if (_tagRepository != null)
        {
            if (_tag?.Id > 0)
                await _tagRepository.DeleteAsync(tag => tag.Id == _tag.Id);

            if (_otherTag?.Id > 0)
                await _tagRepository.DeleteAsync(tag => tag.Id == _otherTag.Id);
        }

        if (_productRepository != null && _products.Count > 0)
        {
            var ids = _products.Select(product => product.Id).ToArray();
            // Predicate-based delete physically removes products, including soft-deleted ones.
            await _productRepository.DeleteAsync(product => ids.Contains(product.Id));
        }

        _products.Clear();
    }

    [Test]
    public async Task UpdateBulkTags_AddsTagToProduct()
    {
        var changed = await _productTagService.UpdateBulkProductTagAssignmentsAsync(
            _tag.Id, [_products[0].Id], []);

        changed.Should().Be(1);
        MappingExists(_tag.Id, _products[0].Id).Should().BeTrue();
    }

    [Test]
    public async Task UpdateBulkTags_RemovesTagFromProduct()
    {
        await AddMappingAsync(_tag.Id, _products[0].Id);

        var changed = await _productTagService.UpdateBulkProductTagAssignmentsAsync(
            _tag.Id, [], [_products[0].Id]);

        changed.Should().Be(1);
        MappingExists(_tag.Id, _products[0].Id).Should().BeFalse();
    }

    [Test]
    public async Task UpdateBulkTags_PreservesOtherTags()
    {
        await AddMappingAsync(_otherTag.Id, _products[0].Id);
        await AddMappingAsync(_tag.Id, _products[0].Id);

        var changed = await _productTagService.UpdateBulkProductTagAssignmentsAsync(
            _tag.Id, [], [_products[0].Id]);

        changed.Should().Be(1);
        MappingExists(_tag.Id, _products[0].Id).Should().BeFalse();
        MappingExists(_otherTag.Id, _products[0].Id).Should().BeTrue();
    }

    [Test]
    public async Task UpdateBulkTags_ConflictingProductIds_ThrowsException()
    {
        Func<Task> act = async () => await _productTagService.UpdateBulkProductTagAssignmentsAsync(
            _tag.Id, [_products[0].Id], [_products[0].Id]);

        await act.Should().ThrowAsync<ArgumentException>();
        MappingExists(_tag.Id, _products[0].Id).Should().BeFalse();
    }

    [Test]
    public async Task UpdateBulkTags_NoChanges_ReturnsZero()
    {
        await AddMappingAsync(_tag.Id, _products[0].Id);

        var changed = await _productTagService.UpdateBulkProductTagAssignmentsAsync(
            _tag.Id, [], []);

        changed.Should().Be(0);
        MappingExists(_tag.Id, _products[0].Id).Should().BeTrue();
    }

    private async Task AddMappingAsync(int tagId, int productId)
    {
        await _mappingRepository.InsertAsync(new ProductProductTagMapping
        {
            ProductTagId = tagId,
            ProductId = productId
        });
    }

    private bool MappingExists(int tagId, int productId)
    {
        return _mappingRepository.Table.Any(mapping =>
            mapping.ProductTagId == tagId && mapping.ProductId == productId);
    }
}
