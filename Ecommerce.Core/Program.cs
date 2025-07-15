using System.Data;
using System.Text;
using Ecommerce.Core.BackgroundServices;
using Ecommerce.Core.Hub;
using Ecommerce.Core.Utils;
using Ecommerce.Repository.implementation;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Service.implementation;
using Ecommerce.Service.interfaces;
using Ecommerce.Service.interfaces.implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); 
builder.Services.AddHostedService<NotificationCleanupService>();
builder.Services.AddHostedService<OfferExpireCleanupService>();

// db connection string
if (builder.Environment.IsEnvironment("Test"))
{
    builder.Services.AddDbContext<EcommerceContext>(options =>
        options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}"));
    builder.Services.AddScoped<IDbConnection>(sp =>
    {
        var conn = new SqliteConnection("DataSource=:memory:");
        conn.Open();
        return conn;
    });
}
else
{
    builder.Services.AddDbContext<EcommerceContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// generic part
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();


// services injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<IProductService, ProductService>(); 
builder.Services.AddScoped<IOrderService, OrderService>(); 

// repositories injection
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IFeatureRepository, FeatureRepository>();
builder.Services.AddScoped<IFavouriteRepository, FavouriteRepository>();
builder.Services.AddScoped<ICartRepository, CartRepository>();
builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
builder.Services.AddScoped<IPasswordResetRequestRepository, PasswordResetRequestRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<ICountryRepository, CountryRepository>();
builder.Services.AddScoped<IStateRepository, StateRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IContactUsRepository, ContactUsRepository>();
builder.Services.AddScoped<IImageRepository, ImageRepository>();
builder.Services.AddScoped<IOfferRepository, OfferRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();



// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// jwt authorization
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Request.Cookies.TryGetValue("auth_token", out string? token);
            if (string.IsNullOrEmpty(token))
            {
                token = context.HttpContext.Session.GetString("auth_token");
            }
            context.Token = token;
            return Task.CompletedTask;
        },
        OnChallenge = context =>
        {
            // Avoid default response
            context.HandleResponse();

            HttpRequest? req = context.HttpContext.Request;
            string? path = req.Path + req.QueryString;

            // Preventing infinite loop if already on login
            if (!req.Path.StartsWithSegments("/Login"))
            {
                string encryptedReturnUrl = AesEncryptionHelper.EncryptString(path);
                string loginUrl = $"/Login/Index?ReturnURL={encryptedReturnUrl}";
                context.Response.Redirect(loginUrl);
            }
            else
            {
                context.Response.Redirect("/Login/Index");
            }
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // app.UseExceptionHandler("/Login/Error");
    // // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    // app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); 

app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TokenRefreshMiddleware>();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401)
    {
        string path = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
        string encryptedReturnUrl = AesEncryptionHelper.EncryptString(path);
        string loginUrl = $"/Login/Index?ReturnURL={encryptedReturnUrl}";
        context.HttpContext.Response.Redirect(loginUrl);
    }
    else if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/Login/Error403");
    }
    else if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Login/Error404");
    }
    await Task.CompletedTask;
});




app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BuyerDashboard}/{action=Index}/{id?}");

// map for hub
app.MapHub<NotificationHub>("/notificationHub");
app.Run();

public partial class Program { }