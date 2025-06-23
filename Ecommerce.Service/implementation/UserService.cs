using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
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

    private readonly IEmailService _emailService;


    public UserService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IEmailService emailService
        )
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _emailService = emailService;

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
                
                DateTime tokenExpire = model.RememberMe 
                    ? DateTime.UtcNow.AddDays(30) // Token expiration time set to 30 days for persistent login
                    : DateTime.UtcNow.AddMinutes(60); // Token expiration time set to 60 minutes
                
                string jwtToken = GenerateJwtToken(model.Email, tokenExpire, userRole ?? "");

                return new ResponseTokenViewModel
                {
                    token = jwtToken,
                    isPersistent = model.RememberMe,
                    response = "Login successful"
                };
            }

            return new ResponseTokenViewModel
            {
                token = null,
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
    private string GenerateJwtToken(string email, DateTime expiryTime, string RoleName)
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

    /// <summary>
    /// inorder to refresh token if its going to expire in few minutes
    /// </summary>
    /// <param name="email"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public ResponseTokenViewModel RefreshToken(string email, string role)
    {
        try
        {
            DateTime tokenExpire = DateTime.UtcNow.AddDays(30); // Or 30 days for persistent
            string jwtToken = GenerateJwtToken(email, tokenExpire, role);
            return new ResponseTokenViewModel
            {
                token = jwtToken,
                sessionId = null,
                isPersistent = false,
                response = "Token refreshed successfully"
            };
        }
        catch (Exception e)
        {
            throw new Exception($"Error in RefreshToken: {e.Message}");
        }
    }

    /// <summary>
    /// method for generating link for reset password
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    public async Task<ResponsesViewModel> ForgotPassword(EmailViewModel email)
    {
        try
        {
            User? user = _userRepository.GetUserByEmail(email.ToEmail.Trim().ToLower());
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            // main logic for forget password
            PasswordResetRequest passwordResetRequest = new PasswordResetRequest()
            {
                Userid = user.UserId,
                Createdate = DateTime.Now,
                Guidtoken = Guid.NewGuid().ToString()
            };
            
            _userRepository.AddPasswordResetRequest(passwordResetRequest);
            
            // reset password url for email
            string BaseUrl = _configuration["UrlSettings:BaseUrl"] ?? "";
            string ResetPasswordUrl = $"{BaseUrl}Home/ResetPassword?token={passwordResetRequest.Guidtoken}";
            string ResetLink = HtmlEncoder.Default.Encode(ResetPasswordUrl);

            string EmailBody = $@"
                                <html>
                                <body>
                                    <h1>Password Reset Request</h1>
                                    <p>Dear {user.UserName},</p>
                                    <p>We received a request to reset your password. Please click the link below to reset your password:</p>
                                    <a href='{ResetLink}' style='color:blue;'>Reset Password</a>
                                    <p style='color:red;'>This link will expire in 1 hour.</p>
                                    <p>If you did not request this change, please ignore this email.</p>
                                    <p>Thank you!</p>
                                </body>
                                </html>";
            
            await _emailService.SendEmailAsync(
                    user.Email,
                    "Password Reset Request",
                    EmailBody
                );
            
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "Reset password email sent successfully, please check your emails" // when reset password email is sent
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }


    /// <summary>
    /// method for validate reset password link 
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public ResponsesViewModel ValidateResetPasswordToken(string token)
    {
        try
        {
            // check if token is null or empty
            if (string.IsNullOrEmpty(token))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid password reset link" // when token is null or empty
                };
            }

            PasswordResetRequest? resetRequest = _userRepository.GetPasswordResetRequestByToken(token);
            // check if token is not found in the database
            if (resetRequest == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid password reset link" // when token is not found
                };
            }

            // check if token is closed or expired
            if (resetRequest.Closedate != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Password reset link has expired" // when token is expired
                };
            }

            // check if token is created more than 1 hour ago
            if (resetRequest.Createdate < DateTime.Now.AddHours(-1))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Password reset link has expired" // when token is expired
                };
            }
            
            // check if user exists for the token
            User? user = _userRepository.GetUserById(resetRequest.Userid);
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }

            // if all checks are passed, return success
            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = user.Email // when token is valid
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    /// <summary>
    /// resets password 
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public ResponsesViewModel ResetPassword(ForgetPasswordViewModel model)
    {
        try
        {
            // check if model is null or empty
            if (model == null || string.IsNullOrEmpty(model.Token) || string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Invalid request" // when model is null or empty
                };
            }

            // validate the reset password token
            ResponsesViewModel response = ValidateResetPasswordToken(model.Token);
            if (!response.IsSuccess)
            {
                return response; // return error response if token is invalid
            }

            // get user by email
            User? user = _userRepository.GetUserByEmail(model.Email.Trim().ToLower());
            if (user == null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User not found" // when user is not found
                };
            }
            // check if password is same as old password
            if (BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "New password cannot be same as old password" // when new password is same as old password
                };
            }

            // update user password
            user.Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password);
            _userRepository.UpdateUser(user);

            // close the reset password request
            PasswordResetRequest? resetRequest = _userRepository.GetPasswordResetRequestByToken(model.Token);
            if (resetRequest != null)
            {
                resetRequest.Closedate = DateTime.Now;
                _userRepository.UpdatePasswordResetRequest(resetRequest);
            }

            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "Password reset successfully" // when password is reset successfully
            };
        }
        catch (Exception ex)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error 500 : Internal Server Error {ex.Message}"
            };
        }
    }

    /// <summary>
    /// method for registering a new user
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public ResponsesViewModel RegisterUser(RegisterUserViewModel model)
    {
        try
        {
            // Check if user already exists
            User? existingUser = _userRepository.GetUserByEmail(model.Email.Trim().ToLower());
            if (existingUser != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User with this email already exists"
                };
            }
            
            Profile profile = new Profile 
            {
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                Pincode = model.Pincode,
                CountryId = model.CountryId,
                StateId = model.StateId,
                CityId = model.CityId,
                FirstName = model.FirstName,
                LastName = model.LastName,
                CreatedAt = DateTime.Now
            };
            _userRepository.AddProfile(profile);

            // Create new user
            User newUser = new User
            {
                UserName = model.UserName,
                Email = model.Email.Trim().ToLower(),
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword(model.Password),
                RoleId = model.RoleId,
                ProfileId = profile.ProfileId,
                CreatedAt = DateTime.Now
            };
            _userRepository.AddUser(newUser);

            return new ResponsesViewModel()
            {
                IsSuccess = true,
                Message = "User registered successfully"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel()
            {
                IsSuccess = false,
                Message = $"Error in RegisterUser: {e.Message}"
            };
        }
    }

    /// <summary>
    /// method for getting list of countries
    /// </summary>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<Country>? GetCountries()
    {
        try
        {
            return _userRepository.GetCountries() ?? new List<Country>();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    
    /// <summary>
    /// method for getting states based on country id
    /// </summary>
    /// <param name="countryId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<State>? GetStates(int countryId)
    {
        try
        {
            return _userRepository.GetStates(countryId) ?? new List<State>();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }

    /// <summary>
    /// method for getting cities based on state id
    /// </summary>
    /// <param name="stateId"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public List<City>? GetCities(int stateId)
    {
        try
        {
            return _userRepository.GetCities(stateId) ?? new List<City>();
        }
        catch(Exception e)
        {
            throw new Exception(e.Message);
        }
    }


    /// <summary>
    /// method for getting user + profile data using email
    /// </summary>
    /// <param name="email"></param>
    /// <returns> returns EditRegisteredUserViewModel </returns>
    public EditRegisteredUserViewModel? GetUserDetailsByEmail(string email)
    {
        try
        {
            EditRegisteredUserViewModel? model = _userRepository.GetUserDetailsByEmail(email);
            return model ?? new EditRegisteredUserViewModel();
        }
        catch(Exception e)
        {
            throw new Exception("error occured while fatching user : " + e.Message);
        }
    }

    /// <summary>
    /// method for edit user and profile
    /// </summary>
    /// <param name="model"></param>
    /// <returns>ResponsesViewModel</returns>
    public ResponsesViewModel EditUserDetails(EditRegisteredUserViewModel model)
    {
        try
        {   
            if(model!= null)
            {
                User? user = _userRepository.GetUserById(model.UserId);
                Profile? profile = _userRepository.GetProfileById(model.ProfileId);

                if(user!=null && profile!=null && user.ProfileId == model.ProfileId)
                {
                    // profile update //
                    profile.Address = model.Address;
                    profile.Pincode = model.Pincode;
                    profile.CityId = model.CityId;
                    profile.StateId = model.StateId;
                    profile.CountryId = model.CountryId;
                    profile.PhoneNumber = model.PhoneNumber;
                    profile.EditedAt = DateTime.Now;

                    _userRepository.UpdateProfile(profile);

                    // user update //
                    user.EditedAt = DateTime.Now;
                    
                    _userRepository.UpdateUser(user);
                    
                    return new ResponsesViewModel{
                        IsSuccess = true,
                        Message = $"user details edited successfully!"
                    };
                }
                return new ResponsesViewModel{
                        IsSuccess = false,
                        Message = $"Error in EditUserDetails!"
                    };
            }
            return new ResponsesViewModel{
                    IsSuccess = false,
                    Message = $"Error in EditUserDetails!"
                };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error in EditUserDetails: {e.Message}"
            };
        }
    }

}
