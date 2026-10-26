using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents product search parameters for bulk product tag management.
/// </summary>
public partial record BulkProductTagSearchModel : ProductSearchModel
{
    public int ProductTagId { get; set; }

    public string ProductTagName { get; set; }

    public bool CanEditTags { get; set; }
}
