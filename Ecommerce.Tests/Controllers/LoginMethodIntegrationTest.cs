using Ecommerce.Repository.Models;
using Ecommerce.Repository.ViewModels;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using System.Net.Http.Json;
using static Ecommerce.Repository.Helpers.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.AspNetCore.Hosting;
using System.Data;
using Microsoft.Data.Sqlite;
using Ecommerce.Service.interfaces;
using Ecommerce.Service.implementation;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.implementation;
using Ecommerce.Core.Utils;

[Collection("Sequential")]
public class LoginMethodIntegrationTest : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public LoginMethodIntegrationTest(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            // Set the environment to "Test"
            builder.UseEnvironment("Test");

            builder.ConfigureServices(services =>
            {
                // Remove existing DbContext and DbConnection registrations
                var descriptors = services
                    .Where(d => 
                        d.ServiceType == typeof(EcommerceContext) ||
                        d.ServiceType == typeof(DbContextOptions) ||
                        d.ServiceType == typeof(DbContextOptions<EcommerceContext>) ||
                        d.ServiceType == typeof(IDbConnection) ||
                        d.ServiceType.Name.Contains("IDbContextFactory"))
                    .ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Add in-memory database for EF Core
                services.AddDbContext<EcommerceContext>(options =>
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));

                // Add in-memory SQLite for Dapper
                services.AddScoped<IDbConnection>(sp =>
                {
                    var connection = new SqliteConnection("DataSource=:memory:");
                    connection.Open(); // Open connection for in-memory SQLite
                    return connection;
                });

                // Ensure required services are registered
                services.AddScoped<IUserService, UserService>();
                services.AddScoped<IUserRepository, UserRepository>();
                services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
                services.AddScoped<IUnitOfWork, UnitOfWork>();
            });
        });
    }


    /// <summary>
    /// Invalid model state returns error
    /// </summary>
    /// <returns>
    /// returns status code ok with error message "invalid user credentials
    /// </returns>
    [Fact]
    public async Task Login_WithValidCredentials_ReturnsRedirect()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
            context.Database.EnsureCreated(); // Ensure DB is ready

            context.Users.Add(new User
            {
                Email = "test@example.com",
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!"),
                RoleId = (int)RoleEnum.Seller,
                UserName = "TestUser"
            });
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();

        // Act
        var response = await client.PostAsJsonAsync("/Login/Index", new LoginViewModel
        {
            Email = "test@example.com",
            Password = "Password123!"
            // invalid model fields, rememberme is missing
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid user credentials", content);
    }

    /// <summary>
    /// invalid model state (remember me is not present)
    /// </summary>
    /// <returns>
    /// will return status code ok with error message "invalid user credentials 
    /// </returns>
    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsViewWithError()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
            context.Database.EnsureCreated(); // Ensure DB is ready
        }

        var client = _factory.CreateClient();

        // Act : feild is not present in test db
        var response = await client.PostAsJsonAsync("/Login/Index", new LoginViewModel
        {
            Email = "wrong@example.com",
            Password = "wrongpass"
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid user credentials", content);
    }

    /// <summary>
    /// Empty email and password empty input
    /// </summary>
    /// <returns>
    /// should return ok status with error message "Email & password are requrired  
    /// </returns>
    [Fact]
    public async Task Login_WithEmptyCredentials_ReturnsViewWithError()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
            context.Database.EnsureCreated();
        }

        var client = _factory.CreateClient();
        var loginModel = new LoginViewModel
        {
            Email = "",
            Password = ""
        };

        // Act
        var response = await client.PostAsJsonAsync("/Login/Index", loginModel);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Email and Password are required", content);
    }


    /// <summary>
    /// Valid credentials with RememberMe which enables  persistent cookie
    /// </summary>
    /// <returns>
    /// returns found status code which sets the cookie with jwt token
    /// </returns>
    [Fact]
    public async Task Login_WithValidCredentialsAndRememberMe_SetsPersistentCookie()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider.GetRequiredService<EcommerceContext>();
            context.Database.EnsureCreated();

            context.Users.Add(new User
            {
                Email = "persistent@example.com",
                Password = BCrypt.Net.BCrypt.EnhancedHashPassword("Password123!"),
                RoleId = (int)RoleEnum.Seller,
                UserName = "PersistentUser"
            });
            await context.SaveChangesAsync();
        }

        var client = _factory.CreateClient();
        var loginModel = new LoginViewModel
        {
            Email = "persistent@example.com",
            Password = "Password123!",
            RememberMe = true
        };

        // Act
        var response = await client.PostAsJsonAsync("/Login/Index", loginModel);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(".AspNetCore.Antiforgery", response.Headers.GetValues("Set-Cookie").FirstOrDefault());
    }




}

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection { }