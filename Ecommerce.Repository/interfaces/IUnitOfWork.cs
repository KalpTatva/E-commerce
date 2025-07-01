namespace Ecommerce.Repository.interfaces;

public interface IUnitOfWork : IDisposable
{
    ICartRepository CartRepository {get; set;}
    IFavouriteRepository FavouriteRepository {get;set;}
    IFeatureRepository FeatureRepository {get; set;}
    INotificationRepository NotificationRepository {get;set;}
    IUserNotificationMappingRepository UserNotificationMappingRepository {get;set;}
    IUserRepository UserRepository {get;set;}
    IPasswordResetRequestRepository PasswordResetRequestRepository {get;set;}
    IProfileRepository ProfileRepository {get;set;}
    ICountryRepository CountryRepository {get;set;}
    IStateRepository StateRepository {get;set;}
    ICityRepository CityRepository {get;set;}
    IContactUsRepository ContactUsRepository {get;set;}
    IProductRepository ProductRepository {get;set;}
    IImageRepository ImageRepository {get;set;}
    IOfferRepository OfferRepository {get;set;}
    IOrderRepository OrderRepository {get;set;}
    IReviewRepository ReviewRepository {get;set;}
    IOrderProductRepository OrderProductRepository {get;set;}
    Task<int> SaveChanges();
}
