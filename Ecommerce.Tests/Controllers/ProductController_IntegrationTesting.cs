// using System.Data;
// using System.IdentityModel.Tokens.Jwt;
// using System.Net.Http.Headers;
// using System.Net.Http.Json;
// using System.Security.Claims;
// using System.Text;
// using System.Text.Json;
// using Ecommerce.Core.Controllers;
// using Ecommerce.Core.Utils;
// using Ecommerce.Repository.implementation;
// using Ecommerce.Repository.interfaces;
// using Ecommerce.Repository.Models;
// using Ecommerce.Service.implementation;
// using Ecommerce.Service.interfaces;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.AspNetCore.TestHost;
// using Microsoft.Data.Sqlite;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.IdentityModel.Tokens;
// using static Ecommerce.Repository.Helpers.Enums;

// namespace Ecommerce.Tests.Controllers;

// public class ProductController_IntegrationTesting : IClassFixture<WebApplicationFactory<Program>>
// {
//     private readonly WebApplicationFactory<Program> _factory;

//     public ProductController_IntegrationTesting(WebApplicationFactory<Program> factory)
//     {
//         _factory = factory.WithWebHostBuilder(builder =>
//             { 
//                 builder.UseEnvironment("Test");
//                 builder.ConfigureTestServices(services => 
//                 {
//                     var descriptors = services.Where(d => 
//                         d.ServiceType == typeof(EcommerceContext) ||
//                         d.ServiceType == typeof(DbContextOptions) ||
//                         d.ServiceType == typeof(DbContextOptions<EcommerceContext>) ||
//                         d.ServiceType == typeof(IDbConnection) ||
//                         d.ServiceType.Name.Contains("IDbContextFactory"))
//                     .ToList();

//                     foreach (var descriptor in descriptors)
//                     {
//                         services.Remove(descriptor);
//                     }

//                     services.AddDbContext<EcommerceContext>(options =>
//                     options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

//                     services.AddScoped<IDbConnection>(sp =>
//                     {
//                         var connection = new SqliteConnection("DataSource=:memory:");
//                         connection.Open();
//                         return connection;
//                     });

//                     services.AddScoped<IUserService, UserService>();
//                     services.AddScoped<IProductService, ProductService>();
//                     services.AddScoped<IUserRepository, UserRepository>();
//                     services.AddScoped<IProductRepository, ProductRepository>();
//                     services.AddScoped<IImageRepository, ImageRepository>();
//                     services.AddScoped<IFeatureRepository, FeatureRepository>();
//                     services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
//                     services.AddScoped<IUnitOfWork, UnitOfWork>();

//                 });
//             }
//         );
//     }

//     private string GenerateJwtToken(string email, string role)
//     {
//         var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("4d3ade891d0a1b178da032c5b578ff0dd9972301034543c82d2dc8e458ad606b0067d35c04551de6e91ce4905113c68cb50a1a571f4aafe53ea9a9c93fcbc188232f7954acba0b6f767e247208005e9e97bc5c417403eb67a195b8c98727fbfafcf7bdbdfa9825b371d345635465aa4a6073142b330274f0c6772309d815ceface714e5e881f89da15322b79c33736d5237603ad1f7e4555c0c856d6b7e30c4a0fc86c39ceb17983baf5020ec8c4b811b2f8bd594e65acfe8f4a1266d5c439af862bb67f0d1639c355e7f7be26301ffa4c30e939f63874ee0f8a6da25cea193f968b6165670a633fbdcb19ee0299d555e7ce122b9808ba6f88cfabf0ed766eeb"));
//         var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//         var claims = new[] { new Claim(JwtRegisteredClaimNames.Email, email), new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), new Claim(ClaimTypes.Role, role), new Claim(JwtRegisteredClaimNames.Name, "SellerUser") };
//         var token = new JwtSecurityToken(issuer: "AppIssuer", audience: "Audience", claims: claims, expires: DateTime.UtcNow.AddMinutes(60), signingCredentials: creds);
//         return new JwtSecurityTokenHandler().WriteToken(token);
//     }


//     /// <summary>
//     /// add product with valid data 
//     /// </summary>
//     /// <returns>
//     /// returns success with status code ok
//     /// </returns>
//     [Fact]
//     public async Task AddProduct_WithValidData_ReturnsSuccess()
//     {
//         // Arrange
//         using (var scope = _factory.Services.CreateScope())
//         {
//             var context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
//             context.Database.EnsureCreated();
//             context.Users.Add(new User { Email = "seller@example.com", Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!"), RoleId = (int)RoleEnum.Seller, UserName = "SellerUser", UserId = 1 });
//             await context.SaveChangesAsync();
//         }

//         string token = GenerateJwtToken("seller@example.com", "Seller");
//         var httpContext = new DefaultHttpContext { Session = new MockHttpSession(), User = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim(JwtRegisteredClaimNames.Email, "seller@example.com"), new Claim(ClaimTypes.Role, "Seller"), new Claim(JwtRegisteredClaimNames.Name, "SellerUser") }, "Bearer")) };
//         SessionUtils.SetSession(httpContext, "auth_token", token);
//         var client = _factory.WithWebHostBuilder(builder => builder.ConfigureTestServices(services => 
//         { 
//             services.AddSingleton<IHttpContextAccessor>(new HttpContextAccessor { HttpContext = httpContext }); 
//             services.AddSession(options => options.IdleTimeout = TimeSpan.FromMinutes(30)); 
//             services.AddControllers().AddApplicationPart(typeof(ProductController).Assembly).AddControllersAsServices(); // Explicitly register controller
//         }).Configure(app => 
//         { 
//             app.UseSession(); 
//             app.UseRouting(); 
//             app.UseAuthentication(); 
//             app.UseAuthorization(); 
//             app.Use(next => async context => 
//             { 
//                 SessionUtils.SetSession(context, "auth_token", token); 
//                 Console.WriteLine($"Session Token Set: {SessionUtils.GetSession(context, "auth_token")}"); // Debug session
//                 await next(context); 
//             }); 
//             app.UseEndpoints(endpoints => 
//             { 
//                 endpoints.MapControllers(); 
//                 Console.WriteLine("Controller endpoints mapped"); // Debug routing
//             }); 
//         })).CreateClient();
//         client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

//         var content = new MultipartFormDataContent
//         {
//             { new StringContent("Test Product"), "ProductName" },
//             { new StringContent("A test product description"), "Description" },
//             { new StringContent("1"), "CategoryId" },
//             { new StringContent("100"), "Price" },
//             { new StringContent("10"), "Stocks" },
//             { new StringContent("10"), "Discount" },
//             { new StringContent("1"), "DiscountType" },
//             { new StringContent(JsonSerializer.Serialize(new List<Feature> { new Feature { FeatureName = "Color", Description = "Red" } })), "FeaturesInput" }
//         };

//         // Act 
//         var response = await client.PostAsync("/Product/AddProduct", content);

//         // Assert
//         if (response.StatusCode != System.Net.HttpStatusCode.OK)
//         {
//             var responseContent = await response.Content.ReadAsStringAsync();
//             Console.WriteLine($"Response Status: {response.StatusCode}, Content: {responseContent}");
//         }
//         Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
//         var jsonResponse = await response.Content.ReadFromJsonAsync<JsonElement>();
//         Assert.True(jsonResponse.GetProperty("success").GetBoolean());
//         Assert.Equal("Product added successfully!", jsonResponse.GetProperty("message").GetString());
//     }



//     public class MockHttpSession : ISession
//     {
//         private readonly Dictionary<string, byte[]> _store = new();
//         public bool IsAvailable => true;
//         public string Id => Guid.NewGuid().ToString();
//         public IEnumerable<string> Keys => _store.Keys;
//         public void Clear() => _store.Clear();
//         public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
//         public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
//         public void Remove(string key) => _store.Remove(key);
//         public void Set(string key, byte[] value) => _store[key] = value;
//         public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value);
//     }

// }
