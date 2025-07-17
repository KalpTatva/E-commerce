using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.interfaces;

public interface IGrantOfferPermissionRepository : IGenericRepository<GrantOfferPermission>
{ 
    Task<string?> GetGrantedCategoryString(int userId);
    
}
