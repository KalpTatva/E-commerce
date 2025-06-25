using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IFeatureRepository
{

    /// <summary>
    /// method for getting features of perticular product by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    List<Feature>? GetFeaturesByProductId(int productId);

    /// <summary>
    /// hard delete on features
    /// </summary>
    /// <param name="feature"></param>
    void DeleteFeature(Feature feature);

    /// <summary>
    /// method for add new feature
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    void AddFeature(Product product);

     /// <summary>
    /// method for updating range of features 
    /// </summary>
    /// <param name="features"></param>
    /// <exception cref="Exception"></exception>
    void updateFeaturesRange(List<Feature> features);

    /// <summary>
    /// method for adding range of features into feature table
    /// </summary>
    /// <param name="features"></param>
    void AddFeaturesRange(List<Feature> features);
}
