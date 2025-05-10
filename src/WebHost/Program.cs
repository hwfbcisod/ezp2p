using EasyP2P.Infrastructure;
using Infrastructure.Sql;
using Infrastructure.Sql.Interfaces;

namespace WebHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
            .AddUserSecrets<Program>();

        // Add services to the container.
        builder.Services.AddSingleton<IStateMachineRepository, PostgresStateMachineRepository>();
        builder.Services.AddScoped<IStateMachineManager, StateMachineManager>();
        builder.Services.AddControllersWithViews();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        app.UseStaticFiles();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");

        app.Run();
    }
}
