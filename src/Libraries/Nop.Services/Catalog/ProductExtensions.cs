using System.Linq.Expressions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Localization;
using Nop.Data;

namespace Nop.Services.Catalog;

public static class ProductExtensions
{
    #region Utilities

    private static IQueryable<Product> OrderByIndexedResults(this IQueryable<Product> productsQuery, IList<int> indexedResults)
    {
        return
            from product in productsQuery
            join indexed in indexedResults.Select((id, index) => new { Index = index, Id = id }).AsQueryable() on product.Id equals indexed.Id into sortedQuery
            from sorted in sortedQuery.DefaultIfEmpty()
            orderby sorted == null ? int.MaxValue : sorted.Index
            select product;
    }

    /// <summary>
    /// Sort key: the localized product name for the given language, falling back to the non-localized name
    /// </summary>
    private static Expression<Func<Product, string>> LocalizedName(IRepository<LocalizedProperty> repository, int languageId)
    {
        return p => repository.Table
            .Where(lp => lp.EntityId == p.Id && lp.LanguageId == languageId
                && lp.LocaleKeyGroup == nameof(Product) && lp.LocaleKey == nameof(Product.Name))
            .Select(lp => lp.LocaleValue)
            .FirstOrDefault() ?? p.Name;
    }

    #endregion

    /// <summary>
    /// Sorts the elements of a sequence in order according to a product sorting rule
    /// </summary>
    /// <param name="productsQuery">A sequence of products to order</param>
    /// <param name="orderBy">Product sorting rule</param>
    /// <param name="localizedPropertyRepository">Localized property repository; pass null to sort by general (non-localized) product names</param>
    /// <param name="languageId">Language identifier; pass 0 to sort by general (non-localized) product names</param>
    /// <param name="indexedResults">List of product identifiers whose index matches the product's position in the sorted list</param>
    /// <param name="orderByTagId">Optional product tag identifier; products with this tag are listed first, then <paramref name="orderBy"/> is applied. Requires <paramref name="productTagMappingRepository"/></param>
    /// <param name="productTagMappingRepository">Product tag mapping repository; pass null to ignore <paramref name="orderByTagId"/></param>
    /// <returns>An System.Linq.IOrderedQueryable`1 whose elements are sorted according to a rule.</returns>
    /// <remarks>
    /// If <paramref name="orderBy"/> is set to <c>Position</c>, no tag priority is requested and passed <paramref name="productsQuery"/> is ordered sorting rule will be skipped
    /// </remarks>
    public static IQueryable<Product> OrderBy(this IQueryable<Product> productsQuery,
        ProductSortingEnum orderBy,
        IRepository<LocalizedProperty> localizedPropertyRepository = null,
        int languageId = 0,
        IList<int> indexedResults = null,
        int? orderByTagId = null,
        IRepository<ProductProductTagMapping> productTagMappingRepository = null)
    {
        if (orderByTagId is int tagId && productTagMappingRepository is not null)
        {
            var tagged = productsQuery.OrderByDescending(p => productTagMappingRepository.Table.Any(m => m.ProductId == p.Id && m.ProductTagId == tagId));

            return orderBy switch
            {
                ProductSortingEnum.NameAsc when localizedPropertyRepository is not null => tagged.ThenBy(LocalizedName(localizedPropertyRepository, languageId)),
                ProductSortingEnum.NameDesc when localizedPropertyRepository is not null => tagged.ThenByDescending(LocalizedName(localizedPropertyRepository, languageId)),
                ProductSortingEnum.Position when indexedResults?.Any() == true =>
                    from p in productsQuery
                    join i in indexedResults.Select((id, index) => new { Id = id, Index = index }).AsQueryable() on p.Id equals i.Id into sorted
                    from i in sorted.DefaultIfEmpty()
                    orderby productTagMappingRepository.Table.Any(m => m.ProductId == p.Id && m.ProductTagId == tagId) descending,
                        i == null ? int.MaxValue : i.Index
                    select p,
                ProductSortingEnum.NameAsc => tagged.ThenBy(p => p.Name),
                ProductSortingEnum.NameDesc => tagged.ThenByDescending(p => p.Name),
                ProductSortingEnum.PriceAsc => tagged.ThenBy(p => p.Price),
                ProductSortingEnum.PriceDesc => tagged.ThenByDescending(p => p.Price),
                ProductSortingEnum.CreatedOn => tagged.ThenByDescending(p => p.CreatedOnUtc),
                _ => tagged.ThenBy(p => p.DisplayOrder).ThenBy(p => p.Id)
            };
        }

        return orderBy switch
        {
            ProductSortingEnum.NameAsc when localizedPropertyRepository is not null => productsQuery.OrderBy(LocalizedName(localizedPropertyRepository, languageId)),
            ProductSortingEnum.NameDesc when localizedPropertyRepository is not null => productsQuery.OrderByDescending(LocalizedName(localizedPropertyRepository, languageId)),
            ProductSortingEnum.Position when indexedResults?.Any() == true => productsQuery.OrderByIndexedResults(indexedResults),
            ProductSortingEnum.NameAsc => productsQuery.OrderBy(p => p.Name),
            ProductSortingEnum.NameDesc => productsQuery.OrderByDescending(p => p.Name),
            ProductSortingEnum.PriceAsc => productsQuery.OrderBy(p => p.Price),
            ProductSortingEnum.PriceDesc => productsQuery.OrderByDescending(p => p.Price),
            ProductSortingEnum.CreatedOn => productsQuery.OrderByDescending(p => p.CreatedOnUtc),
            ProductSortingEnum.Position when productsQuery is IOrderedQueryable => productsQuery,
            _ => productsQuery.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
        };
    }
}