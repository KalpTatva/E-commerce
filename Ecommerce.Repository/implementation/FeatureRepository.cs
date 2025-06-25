using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class FeatureRepository : IFeatureRepository
{

    private readonly EcommerceContext _context;

    public FeatureRepository(EcommerceContext context)
    {
        _context = context;
    }

    /// <summary>
    /// method for getting features of perticular product by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    public List<Feature>? GetFeaturesByProductId(int productId)
    {
        try
        {
            return _context.Features.Where(x => x.ProductId == productId).OrderByDescending(x => x.FeatureId).ToList();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// hard delete on features
    /// </summary>
    /// <param name="feature"></param>
    public void DeleteFeature(Feature feature)
    {
        try
        {
            _context.Features.Remove(feature);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for add new feature
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    public void AddFeature(Product product)
    {
        try
        {
            _context.Products.Add(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


        /// <summary>
    /// method for updating range of features 
    /// </summary>
    /// <param name="features"></param>
    /// <exception cref="Exception"></exception>
    public void updateFeaturesRange(List<Feature> features)
    {
        try
        {
            _context.Features.UpdateRange(features);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for adding range of features into feature table
    /// </summary>
    /// <param name="features"></param>
    public void AddFeaturesRange(List<Feature> features)
    {
        try
        {
            _context.Features.AddRange(features);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

}
