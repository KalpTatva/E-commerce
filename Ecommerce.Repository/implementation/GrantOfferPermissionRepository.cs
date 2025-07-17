using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class GrantOfferPermissionRepository : GenericRepository<GrantOfferPermission>, IGrantOfferPermissionRepository
{
    private readonly EcommerceContext _context;
    public GrantOfferPermissionRepository(EcommerceContext context) : base(context)
    {
        _context = context;
    }

    public async Task<string?> GetGrantedCategoryString(int userId)
    {
        try
        {
            List<string> res = await _context.GrantOfferPermissions.Where(g => g.UserId == userId)
                                                          .Join(_context.Categories, 
                                                                g => g.CategoryId, 
                                                                c => c.CategoryId,
                                                                (g,c) => c.CategoryName)
                                                          .ToListAsync();

            string result = "";
            if(res.Any())
            {
                foreach(string s in res)
                {
                    result += s + ", ";
                }
                result = result.Substring(0,result.Length-1);
            }
            return result;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while fetching granted permissions of seller.", e);
        }
    }

}
