namespace Ecommerce.Tests.Controllers;
using System.Net.Http.Json;
using System.Text.Json;
using Ecommerce.Core.Controllers;
using Ecommerce.Repository.implementation;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Ecommerce.Service.implementation;
using Ecommerce.Service.interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Ecommerce.Repository.Helpers.Enums;


public class loginController_RegisterUser_IntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public loginController_RegisterUser_IntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureTestServices(services =>
            {
                // Remove existing database services
                var descriptors = services.Where(d =>
                    d.ServiceType == typeof(EcommerceContext) ||
                    d.ServiceType == typeof(DbContextOptions) ||
                    d.ServiceType == typeof(DbContextOptions<EcommerceContext>))
                    .ToList();
                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database
                services.AddDbContext<EcommerceContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                // Register services
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped<IProfileRepository, ProfileRepository>();
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IUnitOfWork, UnitOfWork>();
                services.AddControllers().AddApplicationPart(typeof(LoginController).Assembly).AddControllersAsServices();
            });
        });
    }

    [Fact]
    public async Task RegisterUser_WithValidData_RedirectsToIndex()
    {
        // Arrange
        var client = _factory.CreateClient();
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "UserName", "testuser" },
            { "Email", "test@example.com" },
            { "Password", "Password123!" },
            { "Address", "123 Test Street" },
            { "PhoneNumber", "1234567890" },
            { "Pincode", "12345" },
            { "CountryId", "1" },
            { "StateId", "1" },
            { "CityId", "1" },
            { "FirstName", "Test" },
            { "LastName", "User" },
            { "RoleId", ((int)RoleEnum.Buyer).ToString() }
        });

        // Act
        var response = await client.PostAsync("/RegisterUser", content);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); 
    }
}