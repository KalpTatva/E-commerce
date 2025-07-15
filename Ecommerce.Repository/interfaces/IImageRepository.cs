using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IImageRepository : IGenericRepository<Image>
{
    /// <summary>
    /// method which delete images which are no longer selected
    /// </summary>
    /// <param name="DeletedImageIdList"></param>
    /// <exception cref="Exception"></exception>
    void DeleteProductImagesByIds(List<int> DeletedImageIdList);
}
