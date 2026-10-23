using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents a product and its assignment to the selected product tag.
/// </summary>
public partial record ProductTagProductModel : BaseNopEntityModel
{
    public string Name { get; set; }

    public string Sku { get; set; }

    public string PictureThumbnailUrl { get; set; }

    public bool Published { get; set; }

    public bool IsTagged { get; set; }
}
