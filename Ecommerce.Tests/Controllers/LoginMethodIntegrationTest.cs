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
        });

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid user credentials", content);
    }

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

        // Act
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
}

[CollectionDefinition("Sequential", DisableParallelization = true)]
public class SequentialCollection { }