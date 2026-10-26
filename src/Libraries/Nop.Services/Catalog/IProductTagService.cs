using Nop.Core;
using Nop.Core.Domain.Catalog;

namespace Nop.Services.Catalog;

/// <summary>
/// Product tag service interface
/// </summary>
public partial interface IProductTagService
{
    /// <summary>
    /// Delete a product tag
    /// </summary>
    /// <param name="productTag">Product tag</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteProductTagAsync(ProductTag productTag);

    /// <summary>
    /// Delete product tags
    /// </summary>
    /// <param name="productTags">Product tags</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task DeleteProductTagsAsync(IList<ProductTag> productTags);

    /// <summary>
    /// Gets product tags
    /// </summary>
    /// <param name="productTagIds">Product tags identifiers</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product tags
    /// </returns>
    Task<IList<ProductTag>> GetProductTagsByIdsAsync(int[] productTagIds);

    /// <summary>
    /// Gets all product tags
    /// </summary>
    /// <param name="tagName">Tag name</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product tags
    /// </returns>
    Task<IList<ProductTag>> GetAllProductTagsAsync(string tagName = null);

    /// <summary>
    /// Gets all product tags by product identifier
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product tags
    /// </returns>
    Task<IList<ProductTag>> GetAllProductTagsByProductIdAsync(int productId);

    /// <summary>
    /// Gets product tag
    /// </summary>
    /// <param name="productTagId">Product tag identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product tag
    /// </returns>
    Task<ProductTag> GetProductTagByIdAsync(int productTagId);

    /// <summary>
    /// Inserts a product-product tag mapping
    /// </summary>
    /// <param name="tagMapping">Product-product tag mapping</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task InsertProductProductTagMappingAsync(ProductProductTagMapping tagMapping);

    /// <summary>
    /// Updates the product tag
    /// </summary>
    /// <param name="productTag">Product tag</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateProductTagAsync(ProductTag productTag);

    /// <summary>
    /// Get number of products
    /// </summary>
    /// <param name="productTagId">Product tag identifier</param>
    /// <param name="storeId">Store identifier</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the number of products
    /// </returns>
    Task<int> GetProductCountByProductTagIdAsync(int productTagId, int storeId, bool showHidden = false);

    /// <summary>
    /// Get product count for every linked tag
    /// </summary>
    /// <param name="storeId">Store identifier</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the dictionary of "product tag ID : product count"
    /// </returns>
    Task<Dictionary<int, int>> GetProductCountAsync(int storeId, bool showHidden = false);

    /// <summary>
    /// Update product tags
    /// </summary>
    /// <param name="product">Product for update</param>
    /// <param name="productTags">Product tags</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    Task UpdateProductTagsAsync(Product product, string[] productTags);


    /// <summary>
    /// Searches products for bulk assignment to a single product tag.
    /// </summary>
    /// <param name="productTagId">Selected product tag identifier.</param>
    /// <param name="searchProductName">Optional product name filter.</param>
    /// <param name="pageIndex">Zero-based page index.</param>
    /// <param name="pageSize">Number of products per page.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains a paged list of products.
    /// </returns>
    Task<IPagedList<Product>> SearchProductsForBulkTagManagementAsync(
        int productTagId,
        string searchProductName,
        int pageIndex,
        int pageSize);

    /// <summary>
    /// Updates only the selected tag mappings, preserving other product tags.
    /// </summary>
    /// <param name="productTagId">Selected product tag identifier.</param>
    /// <param name="addProductIds">Product identifiers to assign the tag to.</param>
    /// <param name="removeProductIds">Product identifiers to remove the tag from.</param>
    /// <returns>
    /// A task that represents the asynchronous operation.
    /// The task result contains the number of tag assignments changed.
    /// </returns>
    Task<int> UpdateBulkProductTagAssignmentsAsync(
        int productTagId,
        int[] addProductIds,
        int[] removeProductIds);

}