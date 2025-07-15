using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
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
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(
        IConfiguration configuration,
        IEmailService emailService,
        IUnitOfWork unitOfWork
    )
    {
        _configuration = configuration;
        _emailService = emailService;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// service for user login 
    /// </summary>
    /// <param name="model"></param>
    /// <returns>ResponseTokenViewModel</returns>
    public ResponseTokenViewModel UserLogin(LoginViewModel model)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(model.Email);
            if (user != null && BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password))
            {
                string? userRole = user != null && user.RoleId != 0 ? ((RoleEnum)user.RoleId).ToString() : null;
                
                DateTime tokenExpire = DateTime.UtcNow.AddMinutes(60); // Token expiration time set to 60 minutes
                
                string theme = user?.Theme != null ? ((ThemeEnum)user.Theme).ToString() : "system";

                string jwtToken = GenerateJwtToken(model.Email, tokenExpire, userRole ?? "", user?.UserName ?? "");

                return new ResponseTokenViewModel
                {
                    token = jwtToken,
                    BaseTheme = theme,
                    UserName = user?.UserName ?? "",
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
    /// <returns>string</returns>
    private string GenerateJwtToken(string email, DateTime expiryTime, string RoleName, string UserName)
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
            new Claim(JwtRegisteredClaimNames.Name, UserName)
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
    /// <returns>ResponseTokenViewModel</returns>
    /// <exception cref="Exception"></exception>
    public ResponseTokenViewModel RefreshToken(string email, string role, string UserName)
    {
        try
        {
            DateTime tokenExpire = DateTime.UtcNow.AddMinutes(60); // Token expiration time set to 60 minutes
            string jwtToken = GenerateJwtToken(email, tokenExpire, role, UserName);
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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> ForgotPassword(EmailViewModel email)
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email.ToEmail ?? "");
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
            
            await _unitOfWork.PasswordResetRequestRepository.AddAsync(passwordResetRequest);
            
            // reset password url for email
            string BaseUrl = _configuration["UrlSettings:BaseUrl"] ?? "";
            string ResetPasswordUrl = $"{BaseUrl}Login/ResetPassword?token={passwordResetRequest.Guidtoken}";
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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> ValidateResetPasswordToken(string token)
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
            
            PasswordResetRequest? resetRequest = await _unitOfWork.PasswordResetRequestRepository.FindAsync(x => x.Guidtoken == token);
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
            // User? user = _userRepository.GetUserById(resetRequest.Userid);
            User? user = await _unitOfWork.UserRepository.GetByIdAsync(resetRequest.Userid);

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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> ResetPassword(ForgetPasswordViewModel model)
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
            ResponsesViewModel response = await ValidateResetPasswordToken(model.Token);
            if (!response.IsSuccess)
            {
                return response; // return error response if token is invalid
            }

            // get user by email
            User? user = _unitOfWork.UserRepository.GetUserByEmail(model.Email);
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
            await _unitOfWork.UserRepository.UpdateAsync(user);

            // close the reset password request
            PasswordResetRequest? resetRequest = await _unitOfWork.PasswordResetRequestRepository.FindAsync(x => x.Guidtoken == model.Token);

            if (resetRequest != null)
            {
                resetRequest.Closedate = DateTime.Now;
                await _unitOfWork.PasswordResetRequestRepository.UpdateAsync(resetRequest);
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
    /// <returns>ResponsesViewModel</returns>
    public async Task<ResponsesViewModel> RegisterUser(RegisterUserViewModel model)
    {
        try
        {
            // Check if user already exists
            User? existingUser = _unitOfWork.UserRepository.GetUserByEmail(model.Email);

            if (existingUser != null)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User with this email already exists, please try another"
                };
            } 

            User? existingName = _unitOfWork.UserRepository.GetUserByUserName(model.UserName); 
            if(existingName != null)
            {
               return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "User with this Username already exists, please try another"
                }; 
            }

            // Validate required fields
            if(string.IsNullOrEmpty(model.UserName) || string.IsNullOrEmpty(model.Password) || string.IsNullOrEmpty(model.Email))
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Username, Password and Email are required"
                };
            }
            // check if address is valid or not
            if(model.Address.Trim() == string.Empty || model.Address.Length < 5)
            {
                return new ResponsesViewModel()
                {
                    IsSuccess = false,
                    Message = "Address is required and should be at least 5 characters long"
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
            await _unitOfWork.ProfileRepository.AddAsync(profile);

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
            await _unitOfWork.UserRepository.AddAsync(newUser);

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
    /// <returns>List<Country></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<Country>?> GetCountries()
    {
        try
        {
            return await _unitOfWork.CountryRepository.GetAllAsync() ?? new List<Country>();
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
    /// <returns> List<State></returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<State>?> GetStates(int countryId)
    {
        try
        {
            return await _unitOfWork.StateRepository.FindAllAsync(x => x.CountryId == countryId) ?? new List<State>();
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
    /// <returns>List<City>?</returns>
    /// <exception cref="Exception"></exception>
    public async Task<List<City>?> GetCities(int stateId)
    {
        try
        {
            return await _unitOfWork.CityRepository.FindAllAsync(x => x.StateId == stateId) ?? new List<City>();
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
            EditRegisteredUserViewModel? model = _unitOfWork.UserRepository.GetUserDetailsByEmail(email);
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
    public async Task<ResponsesViewModel> EditUserDetails(EditRegisteredUserViewModel model)
    {
        try
        {   
            if(model!= null)
            {
                // User? user = _userRepository.GetUserById(model.UserId);
                User? user = await _unitOfWork.UserRepository.GetByIdAsync(model.UserId);
                Profile? profile = user!=null ? await _unitOfWork.ProfileRepository.GetByIdAsync(user.ProfileId) : null;


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

                    await _unitOfWork.ProfileRepository.UpdateAsync(profile);

                    // user update //
                    user.EditedAt = DateTime.Now;
                    
                    await _unitOfWork.UserRepository.UpdateAsync(user);
                    
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


    /// <summary>
    /// Method to add a contact message from the user.
    /// This method creates a new Contactu object with the provided details,
    /// adds it to the repository, and sends an email notification to the recipient.
    /// </summary>
    /// <param name="model">ContactUsViewModel containing the contact message details</param>
    public async Task<ResponsesViewModel> AddContactMessage(ContactUsViewModel model)
    {
        try
        {
            if (model == null)
            {
                return new ResponsesViewModel
                {
                    IsSuccess = false,
                    Message = "Invalid contact message"
                };
            }
            
            Product? product = await _unitOfWork.ProductRepository.GetByIdAsync(model.ProductId);

            // add notification into the database
            Notification notification = new Notification
            {
                Notification1 = $"You have received a new contact message from {model.Name} ({model.SenderEmail}) for product {product?.ProductName}. check your email.",
                ProductId = model.ProductId
            };
            // _notificationRepository.AddNotification(notification);
            await _unitOfWork.NotificationRepository.AddAsync(notification);

            // notification in mapping table of user and notification
            List<UserNotificationMapping> userNotificationMappings = new List<UserNotificationMapping>();
            
            userNotificationMappings.Add(new UserNotificationMapping
            {
                UserId = _unitOfWork.UserRepository.GetUserByEmail(model.ReciverEmail)?.UserId ?? 0,
                NotificationId = notification.NotificationId,
                CreatedAt = DateTime.Now
            });

            await _unitOfWork.UserNotificationMappingRepository.AddRangeAsync(userNotificationMappings);


            // add contact details
            Contactu contact = new Contactu
            {
                Name = model.Name,
                SenderEmail = model.SenderEmail,
                ReciverEmail = model.ReciverEmail,
                Subject = model.Subject,
                Message = model.Message,
            };
            await _unitOfWork.ContactUsRepository.AddAsync(contact);

            

            // Send email notification
            string emailBody = $@"
                <html>
                <body>
                    <h1>Contact Message Received</h1>
                    <p>Dear {model.Name},</p>
                    <p>Thank you for reaching out to us. We have received your message:</p>
                    <p><strong>Name:</strong> {model.Name}</p>
                    <p><strong>Email:</strong> {model.SenderEmail}</p>
                    <p><strong>Product: </strong> {product?.ProductName} (product id : {model.ProductId})</p>
                    <p><strong>Subject:</strong> {model.Subject}</p>
                    <p><strong>Message:</strong> {model.Message}</p>
                    <p>We will get back to you shortly.</p>
                    <p>Best regards,</p>
                    <p>Ecommerce</p>
                </body>
                </html>";
            await _emailService.SendEmailAsync(
                model.ReciverEmail,
                model.Subject,
                emailBody
            );

            return new ResponsesViewModel
            {
                IsSuccess = true,
                Message = "Contact message sent successfully"
            };
        }
        catch (Exception e)
        {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error in AddContactMessage: {e.Message}"
            };
        }
    }


    public async Task<ResponsesViewModel> ThemeChange(string theme, string email) 
    {
        try
        {
            User? user = _unitOfWork.UserRepository.GetUserByEmail(email);
            if(user!=null)
            {
                user.Theme = ThemeEnum.dark.ToString() == theme ? (int)ThemeEnum.dark : 
                            ThemeEnum.light.ToString() == theme ? (int)ThemeEnum.light : 
                            (int)ThemeEnum.system;
                user.EditedAt = DateTime.Now;
                await _unitOfWork.UserRepository.UpdateAsync(user);
                
                return new ResponsesViewModel
                {
                    IsSuccess = true,
                    Message = $"Theme updated successfully!"
                };

                            
            }
            else
            {
                // theme changed whithout active user, no theme is preserved
                return new ResponsesViewModel
                {
                    IsSuccess = true,
                    Message = $"Theme updated successfully!"
                };
            }
        } catch (Exception e) {
            return new ResponsesViewModel
            {
                IsSuccess = false,
                Message = $"Error in changing theme: {e.Message}"
            };
        }
    }
}
