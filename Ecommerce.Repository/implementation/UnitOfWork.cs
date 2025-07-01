using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;

namespace Ecommerce.Repository.implementation;

public class UnitOfWork : IUnitOfWork
{
    private readonly EcommerceContext _context;
    public ICartRepository CartRepository {get;set;}
    public IFavouriteRepository FavouriteRepository {get;set;}
    public IFeatureRepository FeatureRepository {get;set;}
    public INotificationRepository NotificationRepository {get;set;}
    public IUserNotificationMappingRepository UserNotificationMappingRepository {get;set;}
    public IUserRepository UserRepository {get;set;}
    public ICountryRepository CountryRepository {get;set;}
    public IStateRepository StateRepository {get;set;}
    public ICityRepository CityRepository {get;set;}
    public IContactUsRepository ContactUsRepository {get;set;}
    public IProfileRepository ProfileRepository {get;set;}
    public IProductRepository ProductRepository {get;set;}
    public IImageRepository ImageRepository {get;set;}
    public IOfferRepository OfferRepository {get;set;}
    public IReviewRepository ReviewRepository {get;set;}
    public IOrderRepository OrderRepository {get;set;}
    public IOrderProductRepository OrderProductRepository {get;set;}
    public IPasswordResetRequestRepository PasswordResetRequestRepository {get;set;}

    public UnitOfWork(EcommerceContext context)
    {
        _context = context;
        CartRepository = new CartRepository(_context);
        FavouriteRepository = new FavouriteRepository(_context);
        FeatureRepository = new FeatureRepository(_context);
        NotificationRepository = new NotificationRepository(_context);
        UserNotificationMappingRepository = new UserNotificationMappingRepository(_context);
        UserRepository = new UserRepository(_context);
        CountryRepository = new CountryRepository(_context);
        StateRepository = new StateRepository(_context);
        CityRepository = new CityRepository(_context);
        ContactUsRepository = new ContactUsRepository(_context);
        ProfileRepository = new ProfileRepository(_context);
        ProductRepository = new ProductRepository(_context);
        ImageRepository = new ImageRepository(_context);
        PasswordResetRequestRepository = new PasswordResetRequestRepository(_context);
        OfferRepository = new OfferRepository(_context);
        ReviewRepository = new ReviewRepository(_context);
        OrderRepository = new OrderRepository(_context);
        OrderProductRepository = new OrderProductRepository(_context);
    }

    public async Task<int> SaveChanges(){
        return await _context.SaveChangesAsync();
    }
    public void Dispose()
    {
        _context.Dispose();
    }
}
