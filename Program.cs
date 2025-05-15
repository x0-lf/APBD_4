using APBD_4.Data;
using APBD_4.Repositories;
using APBD_4.Services;

namespace APBD_4;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        //builder.Services.AddScoped<>;
        
        builder.Services.AddSingleton<IDbConnectionFactory,DbConnectionFactory>();

        builder.Services.AddScoped<IWarehouseRepository, WarehouseRepository>();
        
        builder.Services.AddScoped<IWarehouseService, WarehouseService>();
        
        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
    
        // No need of HTTPS -> unless you want HTTPS:
        // app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}