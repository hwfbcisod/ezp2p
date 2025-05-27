using EasyP2P.Web.Data.Models;
using EasyP2P.Web.Data.Repositories;
using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Data.Stores;
using EasyP2P.Web.Repositories;
using EasyP2P.Web.Services;
using Microsoft.AspNetCore.Identity;

namespace EasyP2P.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add Identity Core services (user-only, no roles) WITHOUT Entity Framework
        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequiredLength = 6;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;

            // User settings
            options.User.RequireUniqueEmail = true;
        })
        .AddUserStore<PostgresUserStore>()
        .AddSignInManager<SignInManager<ApplicationUser>>()
        .AddDefaultTokenProviders();

        // Add cookie authentication
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
            options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
            options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
        })
        .AddCookie(IdentityConstants.ApplicationScheme, options =>
        {
            options.LoginPath = "/Account/Login";
            options.LogoutPath = "/Account/Logout";
            options.AccessDeniedPath = "/Account/AccessDenied";
        })
        .AddCookie(IdentityConstants.ExternalScheme, options =>
        {
            options.Cookie.Name = IdentityConstants.ExternalScheme;
            options.ExpireTimeSpan = TimeSpan.FromMinutes(5);
        });

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        // Repository services
        builder.Services.AddScoped<IPurchaseOrderRequestRepository, PostgresPurchaseOrderRequestRepository>();
        builder.Services.AddScoped<IPurchaseOrderRepository, PostgresPurchaseOrderRepository>();
        builder.Services.AddScoped<ISupplierRepository, PostgresSupplierRepository>();

        // Business services
        builder.Services.AddScoped<IPurchaseOrderRequestService, PurchaseOrderRequestService>();
        builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        builder.Services.AddScoped<ISupplierService, SupplierService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();

        // Auth services
        builder.Services.AddScoped<IUserContextService, UserContextService>();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddMemoryCache();
        builder.Services.AddLogging(builder => {
            builder.AddConsole();
            builder.AddDebug();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        // Authentication & Authorization
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}