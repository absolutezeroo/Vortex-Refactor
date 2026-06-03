// @see com.sulake.habbo.session.product.IProductData

namespace Vortex.Habbo.Session.Product;

/// @see com.sulake.habbo.session.product.IProductData
public interface IProductData
{
    string type { get; }
    string name { get; }
    string description { get; }
}
