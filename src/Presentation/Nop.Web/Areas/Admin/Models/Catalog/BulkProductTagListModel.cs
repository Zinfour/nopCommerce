using Nop.Web.Framework.Models;

namespace Nop.Web.Areas.Admin.Models.Catalog;

/// <summary>
/// Represents the paged product list for bulk product tag management.
/// </summary>
public partial record BulkProductTagListModel : BasePagedListModel<ProductTagProductModel>;
