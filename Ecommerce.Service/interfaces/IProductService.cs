using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Service.interfaces;

public interface IProductService
{
    /// <summary>
    /// method service for adding product
    /// </summary>
    /// <param name="model"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    ResponsesViewModel AddProduct(AddProductViewModel model, string email);

    List<Product>? GetSellerSpecificProductsByEmail(string email);
}
