using EasyP2P.Web.Data.Repositories.Interfaces;
using EasyP2P.Web.Data.Repositories;
using EasyP2P.Web.Repositories;
using EasyP2P.Web.Services;

namespace EasyP2P.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();
        builder.Services.AddScoped<IPurchaseOrderRequestRepository, PostgresPurchaseOrderRequestRepository>();
        builder.Services.AddScoped<IPurchaseOrderRepository, PostgresPurchaseOrderRepository>();
        builder.Services.AddScoped<IPurchaseOrderRequestService, PurchaseOrderRequestService>();
        builder.Services.AddScoped<IPurchaseOrderService, PurchaseOrderService>();
        builder.Services.AddScoped<IDashboardService, DashboardService>();
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
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
