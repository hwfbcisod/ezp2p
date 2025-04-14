using EasyP2P.Infrastructure;
using Infrastructure.Sql;
using Infrastructure.Sql.Interfaces;

namespace WebHost;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        var postgresConnection = "";
        builder.Services.AddSingleton<IStateMachineRepository>(new PostgresStateMachineRepository(postgresConnection));
        builder.Services.AddScoped<IStateMachineManager, StateMachineManager>();
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
