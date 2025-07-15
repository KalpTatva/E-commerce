using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class ImageRepository : GenericRepository<Image> , IImageRepository
{
    private readonly EcommerceContext _context;

    public ImageRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

    /// <summary>
    /// method which delete images which are no longer selected
    /// </summary>
    /// <param name="DeletedImageIdList"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteProductImagesByIds(List<int> DeletedImageIdList)
    {
        try
        {
            List<Image>? imagesToDelete = _context.Images.Where(image => DeletedImageIdList.Contains(image.ImageId)).ToList();
            _context.Images.RemoveRange(imagesToDelete);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
