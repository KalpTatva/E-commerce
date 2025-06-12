using Ecommerce.Repository.ViewModels;

namespace Ecommerce.Service.interfaces;

public interface IUserService
{
    /// <summary>
    /// service for user login 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    ResponseTokenViewModel UserLogin(LoginViewModel model);
}
