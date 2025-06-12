using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using static Ecommerce.Repository.Helpers.Enums;

namespace Ecommerce.Service.implementation;

public class UserService : IUserService
{

    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;


    public UserService(
        IUserRepository userRepository,
        IConfiguration configuration
        )
    {
        _userRepository = userRepository;
        _configuration = configuration;

    }

    /// <summary>
    /// service for user login 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public ResponseTokenViewModel UserLogin(LoginViewModel model)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(model.Email);
            if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                string? userRole = user != null && user.RoleId != 0 ? ((RoleEnum)user.RoleId).ToString() : null;
                DateTime TokenExpire = model.RememberMe == true ? DateTime.Now.AddDays(30) : DateTime.Now.AddDays(1);
                string JwtToken = GenerateJwtToken(model.Email, TokenExpire, userRole ?? "");

                string? sessionId = model.RememberMe ? Guid.NewGuid().ToString() : null;

                return new ResponseTokenViewModel()
                {
                    token = JwtToken,
                    sessionId = sessionId,
                    isPersistent = model.RememberMe,
                    response = "Login successful",
                };
            }

            return new ResponseTokenViewModel()
            {
                token = "",
                sessionId = null,
                response = "Invalid user credentials!"
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error in LoginService: {e.Message}");
        }
    }


    /// <summary>
    /// jwt token generator
    /// </summary>
    /// <param name="email"></param>
    /// <param name="expiryTime"></param>
    /// <param name="RoleName"></param>
    /// <returns></returns>
    private string GenerateJwtToken(string email, DateTime expiryTime, string? RoleName)
    {
        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])
        );
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Role, RoleName), 
        };

        JwtSecurityToken token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiryTime,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

}
