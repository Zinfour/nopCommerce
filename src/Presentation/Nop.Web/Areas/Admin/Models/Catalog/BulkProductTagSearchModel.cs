using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents product search parameters for bulk product tag management.
/// </summary>
public partial record BulkProductTagSearchModel : BaseSearchModel
{
    public int ProductTagId { get; set; }

    public string SearchProductName { get; set; }
}
