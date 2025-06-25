using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Repository.implementation;

public class ProductRepository : IProductRepository
{
    private readonly EcommerceContext _context;

    public ProductRepository(EcommerceContext context)
    {
        _context = context;
    }


    /// <summary>
    /// method for adding product into db
    /// </summary>
    /// <param name="product"></param>
    /// <exception cref="Exception"></exception>
    public void AddProduct(Product product)
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
    /// method for adding multiple images 
    /// </summary>
    /// <param name="images"></param>
    /// <exception cref="Exception"></exception>
    public void AddProductImages(List<Image> images)
    {
        try
        {
            _context.Images.AddRange(images);
            _context.SaveChanges();
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// get method for seller specific products
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<Product>? GetSellerSpecificProducts(int userId){
        try
        {
            return _context.Products.Where(x => x.SellerId == userId && x.IsDeleted == false ).OrderByDescending(x => x.ProductId).ToList();
        }
        catch (Exception e)
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

    /// <summary>
    /// get product by id
    /// </summary>
    /// <param name="product"></param>
    /// <returns>Product</returns>
    public Product? GetProductById(int product)
    {
        try
        {
            return _context.Products.FirstOrDefault(x => x.ProductId == product && x.IsDeleted == false);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for soft delete the product 
    /// which updates isdelete, edit and delete time by self
    /// </summary>
    /// <param name="product"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public void DeleteProduct(Product product)
    {
        try
        {
            product.IsDeleted = true;
            product.EditedAt = DateTime.Now;
            product.DeletedAt = DateTime.Now;
            _context.Products.Update(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
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
    /// method for getting details of product for edit product
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
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
    /// method for update product details
    /// </summary>
    /// <param name="product"></param>
    public void updateProducts(Product product)
    {
        try
        {
            _context.Products.Update(product);
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
    /// method for get all product with search filters
    /// </summary>
    /// <param name="search"></param>
    /// <param name="category"></param>
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// <returns></returns>
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
    /// method for fetch favourites by user and product id
    /// </summary>
    /// <param name="UserId"></param>
    /// <param name="ProductId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Favourite? GetFavouriteByIds(int UserId,int ProductId)
    {
        try
        {
            return _context.Favourites.FirstOrDefault(f => f.UserId == UserId && f.ProductId == ProductId);
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for dropping favourite tupple from db
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void dropFavourite(Favourite favourite)
    {
        try
        {
            _context.Favourites.Remove(favourite);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    
    /// <summary>
    /// method for add tupple in favourite
    /// </summary>
    /// <param name="favourite"></param>
    /// <exception cref="Exception"></exception>
    public void AddFavourite(Favourite favourite)
    {
        try
        {
            _context.Favourites.Add(favourite);
            _context.SaveChanges();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for get favourite tupples from db by user id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<int> GetFavouriteByUserId(int userId)
    {
        try
        {
            return _context.Favourites.Where(f => f.UserId == userId).Select(x => x.ProductId).ToList();

        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }
    

    /// <summary>
    /// method for add in cart
    /// </summary>
    /// <param name="cart"></param>
    /// <exception cref="Exception"></exception>
    public void AddToCart(Cart cart)
    {
        try
        {
            _context.Carts.Add(cart);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method which gets cart data based on user
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    public List<productAtCartViewModel> GetproductAtCart(int userId)
    {
        try
        {
            List<productAtCartViewModel>? query = (
                from product in _context.Products
                join cart in _context.Carts on product.ProductId equals cart.ProductId
                where product.IsDeleted == false && cart.IsDeleted == false &&  cart.UserId == userId
                orderby cart.CartId descending
                select new productAtCartViewModel
                {
                    CartId = cart.CartId,
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    Price = product.Price,
                    Stocks = product.Stocks,
                    Quantity = cart.Quantity,
                    DiscountType = product.DiscountType,
                    Discount = product.Discount,
                    Images = _context.Images.Where(i => i.ProductId == product.ProductId).OrderBy(i => i.ImageId).FirstOrDefault(),
                    Offer = _context.Offers
                        .Where(o => o.ProductId == product.ProductId && 
                                    o.StartDate.Date <= DateTime.Now.Date && 
                                    o.EndDate.Date > DateTime.Now.Date)
                        .FirstOrDefault()
                }).ToList();
            
            return query;
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for updating cart's quantity only
    /// </summary>
    /// <param name="cartId"></param>
    /// <param name="quantity"></param>
    /// <exception cref="Exception"></exception>
    public void UpdateCartById(int cartId,int quantity)
    {
        try
        {
            Cart? cart = _context.Carts.Where(c => c.CartId == cartId).FirstOrDefault();
            if(cart!=null)
            {
                cart.Quantity = quantity;
                _context.Carts.Update(cart);
                _context.SaveChanges();
            }
        
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// soft delete method for updating delete boolean = true
    /// </summary>
    /// <param name="cartId"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteCartById(int cartId)
    {
        try
        {
            Cart? cart = _context.Carts.Where(c => c.CartId == cartId).FirstOrDefault();
            if(cart!=null)
            {
                cart.IsDeleted = true;
                cart.DeletedAt = DateTime.Now;
                _context.Carts.Update(cart);
                _context.SaveChanges();
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// soft delete implementation for cart items 
    /// </summary>
    /// <param name="cartIds"></param>
    /// <exception cref="Exception"></exception>
    public void DeleteCartByIdsRange(List<int> cartIds)
    {
        try
        {
            List<Cart>? carts = _context.Carts.Where(c => cartIds.Contains(c.CartId)).ToList();
            if(carts!=null)
            {
                foreach(Cart cart in carts)
                {
                    cart.IsDeleted = true;
                    cart.DeletedAt = DateTime.Now;
                }
                _context.Carts.UpdateRange(carts);
                _context.SaveChanges();
            }
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for updating product details
    /// </summary>
    /// <param name="product"></param>
    public void UpdateProduct(Product product)
    {
        try
        {
            _context.Products.Update(product);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for adding review in db
    /// </summary>
    /// <param name="review"></param>
    public void AddReview(Review review)
    {
        try
        {
            _context.Reviews.Add(review);
            _context.SaveChanges();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for getting cart by user id and product id
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="productId"></param>
    /// <returns>Cart</returns>
    public Cart? GetCartByUserIdAndProductId(int userId, int productId)
    {
        try
        {
            return _context.Carts.FirstOrDefault(c => c.UserId == userId && c.ProductId == productId && c.IsDeleted == false);
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


    /// <summary>
    /// method for getting offer by product id
    /// </summary>
    /// <param name="productId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Offer? GetOfferByProductId(int productId)
    {
        try
        {
            return _context.Offers.FirstOrDefault(o => o.ProductId == productId && o.StartDate.Date <= DateTime.Now.Date && o.EndDate.Date > DateTime.Now.Date);
        }
        catch (Exception e)
        {
            throw new Exception(e.Message);
        }
    }
}
