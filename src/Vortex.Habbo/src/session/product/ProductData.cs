// @see com.sulake.habbo.session.product.ProductData

namespace Vortex.Habbo.Session.Product;

/// @see com.sulake.habbo.session.product.ProductData
public class ProductData : IProductData
{
    /// @see ProductData.as::ProductData
    public ProductData(string type, string name, string description = "")
    {
        this.type = type;
        this.name = name;
        this.description = description;
    }

    public string type { get; }
    public string name { get; }
    public string description { get; }
}
