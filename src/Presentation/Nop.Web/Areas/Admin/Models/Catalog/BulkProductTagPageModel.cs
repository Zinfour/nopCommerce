using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents the bulk product tag management page.
/// </summary>
public partial record BulkProductTagPageModel : BaseNopModel
{
    public int ProductTagId { get; set; }

    public string ProductTagName { get; set; }

    public bool CanEditTags { get; set; }

    public BulkProductTagSearchModel SearchModel { get; set; } = new();
}
