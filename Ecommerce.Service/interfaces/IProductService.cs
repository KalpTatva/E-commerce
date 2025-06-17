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
    /// <param name="List<Features>"></param>
    /// <returns></returns>
    ResponsesViewModel AddProduct(AddProductViewModel model, string email, List<Feature> features);

    /// <summary>
    /// get seller specific data from db
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    List<Product>? GetSellerSpecificProductsByEmail(string email);

    /// <summary>
    /// soft delete of product
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ResponsesViewModel DeleteProductById(int id);

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    EditProductViewModel? GetProductDetailsById(int productId);

    /// <summary>
    /// method for
    ///     1. delete images which are no longer in model (have list of images for delete)
    ///     2. udpate features (delete all previouse and add all new one)
    ///     3. update product details
    /// </summary>
    /// <param name="model"></param>
    /// <param name="features"></param>
    /// <param name="DeletedImageIdList"></param>
    /// <returns></returns>
    ResponsesViewModel UpdateProductDetails(EditProductViewModel model, List<Feature>? features, List<int>? DeletedImageIdList);

   
}
