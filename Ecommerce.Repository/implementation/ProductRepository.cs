using System.Data;
using Dapper;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class ProductRepository : GenericRepository<Product>,  IProductRepository
{
    private readonly EcommerceContext _context;

    public ProductRepository(EcommerceContext context) : base (context)
    {
        _context = context;
    }

    /// <summary>
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>EditProductViewModel</returns>
    public EditProductViewModel? GetProductDetailsById(int productId)
    {
        try
        {
            EditProductViewModel model = new ();
            var query = (from product in _context.Products
                         where product.ProductId == productId && product.IsDeleted == false
                         select new
                         {
                             Product = product,
                             Features = _context.Features.Where(f => f.ProductId == product.ProductId).ToList(),
                             Images = _context.Images.Where(i => i.ProductId == product.ProductId).ToList()
                         }).FirstOrDefault();

            

            if (query != null)
            {
                model.ProductId = query.Product.ProductId;
                model.ProductName = query.Product.ProductName;
                model.Description = query.Product.Description;
                model.CategoryId = query.Product.CategoryId;
                model.Price = query.Product.Price;
                model.Stocks = query.Product.Stocks;
                model.SellerId = query.Product.SellerId;
                model.DiscountType = query.Product.DiscountType;
                model.Discount = query.Product.Discount;
                model.Features = query.Features ?? new List<Feature>();
                model.Images = query.Images ?? new List<Image>();

                return model;
            }

            return null;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
        

    } 

    /// <summary>
    /// method for get all product with search filters
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns>List<ProductsDeatailsViewModel></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ProductsDeatailsViewModel>?> GetAllProducts(string? search = null, int? category = null)
    {
        try
        {
            List<ProductsDeatailsViewModel>? query = await (from product in _context.Products
                                    where product.IsDeleted == false &&
                                    (string.IsNullOrEmpty(search) || product.ProductName.ToLower().Trim().Contains(search)) &&
                                    (!category.HasValue || product.CategoryId == category)
                                    orderby product.ProductId descending
                                select new ProductsDeatailsViewModel
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName,
                                    Description = product.Description,
                                    Price = product.Price,
                                    Stocks = product.Stocks,
                                    DiscountType = product.DiscountType,
                                    Discount = product.Discount,
                                    CategoryId = product.CategoryId,
                                    SellerId = product.SellerId,
                                    Features = _context.Features.Where(f => f.ProductId == product.ProductId).ToList(),
                                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault(),
                                    AverageRatings = _context.Reviews
                                        .Where(r => r.ProductId == product.ProductId)
                                        .Average(r => r.Ratings) ?? 0,
                                    OfferAvailable = _context.Offers.Any(o => o.ProductId == product.ProductId && 
                                                                              o.StartDate.Date <= DateTime.Now.Date && 
                                                                              o.EndDate.Date > DateTime.Now.Date),
                                    offer = _context.Offers
                                        .Where(o => o.ProductId == product.ProductId && 
                                                    o.StartDate.Date <= DateTime.Now.Date && 
                                                    o.EndDate.Date > DateTime.Now.Date)
                                        .FirstOrDefault()
                                
                                }).ToListAsync();
            return query;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting product details by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns>productDetailsByproductIdViewModel</returns>
    /// <exception cref="Exception"></exception>
    public async Task<productDetailsByproductIdViewModel?> GetProductDetailsByProductId(int productId)
    {
        try
        {
            productDetailsByproductIdViewModel? query = await (from product in _context.Products
                                where product.IsDeleted == false && product.ProductId == productId
                                orderby product.ProductId descending
                                select new productDetailsByproductIdViewModel
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName,
                                    Description = product.Description,
                                    Price = product.Price,
                                    Stocks = product.Stocks,
                                    DiscountType = product.DiscountType,
                                    Discount = product.Discount,
                                    CategoryId = product.CategoryId,
                                    SellerId = product.SellerId,
                                    SellerEmailId = _context.Users
                                                .Where(u => u.UserId == product.SellerId)
                                                .Select(u => u.Email)
                                                .FirstOrDefault(),
                                    Reviews = _context.Reviews
                                                .Where(r => r.ProductId == product.ProductId)
                                                .OrderByDescending(r => r.ReviewId)
                                                .Join(_context.Users, 
                                                       r => r.BuyerId, 
                                                       u => u.UserId, 
                                                       (r, u) => new { r, u })
                                                .Select(x => new ReviewsViewModel
                                                {
                                                    ReviewId = x.r.ReviewId,
                                                    UserEmail = x.u.Email,
                                                    Ratings = x.r.Ratings ?? 0,
                                                    Comments = x.r.Comments,
                                                    CreatedAt = x.r.CreatedAt ?? DateTime.Now,
                                                    UserName = x.u.UserName
                                                }).ToList()
                                    ,
                                    Features = _context.Features.Where(f => f.ProductId == product.ProductId).ToList(),
                                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).ToList(),
                                    offer = _context.Offers
                                        .Where(o => o.ProductId == product.ProductId && 
                                                    o.StartDate.Date <= DateTime.Now.Date && 
                                                    o.EndDate.Date > DateTime.Now.Date)
                                        .FirstOrDefault(),
                                
                                }).FirstOrDefaultAsync();
            return query;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting products which are user's favourite
    /// </summary>
    /// <param name="userId"></param>
    /// <returns>List<ProductsDeatailsViewModel></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<ProductsDeatailsViewModel>?> GetFavouriteProductsByUserId(int userId)
    {
        try
        {
            List<ProductsDeatailsViewModel>? query = await (
                                from product in _context.Products
                                join f in _context.Favourites on product.ProductId equals f.ProductId
                                where product.IsDeleted == false && f.UserId == userId
                                select new ProductsDeatailsViewModel
                                {
                                    ProductId = product.ProductId,
                                    ProductName = product.ProductName,
                                    Description = product.Description,
                                    Price = product.Price,
                                    Stocks = product.Stocks,
                                    DiscountType = product.DiscountType,
                                    Discount = product.Discount,
                                    CategoryId = product.CategoryId,
                                    SellerId = product.SellerId,
                                    Features = _context.Features.Where(f => f.ProductId == product.ProductId).ToList(),
                                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault(),
                                    AverageRatings = _context.Reviews
                                        .Where(r => r.ProductId == product.ProductId)
                                        .Average(r => r.Ratings) ?? 0,
                                    OfferAvailable = _context.Offers.Any(o => o.ProductId == product.ProductId && 
                                            o.StartDate.Date <= DateTime.Now.Date && 
                                            o.EndDate.Date > DateTime.Now.Date)
                                }).ToListAsync();
            return query;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }    

    /// <summary>
    /// method for getting products for offer by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <exception cref="Exception"></exception>
    public List<ProductNameViewModel> GetProductsForOffer(int userId)
    {
        try
        {
            List<ProductNameViewModel> products = _context.Products
                .Where(p => p.SellerId == userId && p.IsDeleted == false)
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ProductNameViewModel
                {
                    id = p.ProductId,
                    name = p.ProductName
                }).ToList();
            return products;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }   
    }

    /// <summary>
    /// method for getting all products for offer
    /// </summary>
    public List<ProductNameViewModel> GetAllProductsForOffer()
    {
        try
        {
            List<ProductNameViewModel> products = _context.Products
                .Where(p => p.IsDeleted == false)
                .OrderByDescending(p => p.ProductId)
                .Select(p => new ProductNameViewModel
                {
                    id = p.ProductId,
                    name = p.ProductName
                }).ToList();
            return products;
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }   
    }
    
}
