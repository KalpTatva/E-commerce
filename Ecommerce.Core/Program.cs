using System.Text;
using Ecommerce.Core.Hub;
using Ecommerce.Repository.implementation;
using Ecommerce.Repository.interfaces;
using Ecommerce.Repository.Models;
using Ecommerce.Service.implementation;
using Ecommerce.Service.interfaces;
using Ecommerce.Service.interfaces.implementation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSignalR(); 

// db connection string
builder.Services.AddDbContext<EcommerceContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// services injection
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IEmailService, EmailService>(); 
builder.Services.AddScoped<IProductService, ProductService>(); 
builder.Services.AddScoped<IOrderService, OrderService>(); 

// repositories injection
builder.Services.AddScoped(typeof(IUserRepository), typeof(UserRepository));
builder.Services.AddScoped(typeof(IProductRepository), typeof(ProductRepository));
builder.Services.AddScoped(typeof(IOrderRepository), typeof(OrderRepository));
builder.Services.AddScoped(typeof(IFeatureRepository), typeof(FeatureRepository));
builder.Services.AddScoped(typeof(IFavouriteRepository), typeof(FavouriteRepository));
builder.Services.AddScoped(typeof(ICartRepository), typeof(CartRepository));
builder.Services.AddScoped(typeof(INotificationRepository), typeof(NotificationRepository));

// Add session services
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
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
            // Try to get token from cookies first (for "Remember Me")
            context.Request.Cookies.TryGetValue("auth_token", out string? token);

            //  if no cookie token
            if (string.IsNullOrEmpty(token))
            {
                token = context.HttpContext.Session.GetString("auth_token");
            }

            // If token is still null or empty, explicitly set context.Token to null
            context.Token = token;
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            // Redirect to login page on authentication failure
            context.Response.Redirect("/Home/Index");
            context.Response.StatusCode = 401; // Unauthorized
            return Task.CompletedTask;
        }
    };
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseStatusCodePages(async context =>
{
    if (context.HttpContext.Response.StatusCode == 401)
    {
        context.HttpContext.Response.Redirect("/Home/Error401"); // Redirect to login for unauthenticated
    }
    else if (context.HttpContext.Response.StatusCode == 403)
    {
        context.HttpContext.Response.Redirect("/Home/Error403"); // Custom 403 page
    }
    else if (context.HttpContext.Response.StatusCode == 404)
    {
        context.HttpContext.Response.Redirect("/Home/Error404");
    }
    await Task.CompletedTask;
});

app.UseSession(); 
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<TokenRefreshMiddleware>();





app.MapControllerRoute(
    name: "default",
    pattern: "{controller=BuyerDashboard}/{action=Index}/{id?}");

// map for hub
app.MapHub<NotificationHub>("/NotificationHub");

app.Run();