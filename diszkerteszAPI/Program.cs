using diszkerteszAPI.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace diszkerteszAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            //Get the connection string from appsettings.Development.json
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

            // Add services to the container.

            //builder.WebHost.UseUrls("http://0.0.0.0:8080");

            //Add Db connection
            //Base code for on-premises PostgreSQL
            //builder.Services.AddDbContext<diszkerteszDbContext>(options => options.UseNpgsql(connectionString));

            //Used for Azure SQL
            var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            sqlConnectionString = "Server=tcp:sqlserver-diszkertesz-001.database.windows.net,1433;Initial Catalog=sqldb-diszkertesz-001;Persist Security Info=False;User ID=feri;Password=Nyul9ag6l3?;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;";
            builder.Services.AddDbContext<diszkerteszDbContext>(options => options.UseAzureSql(sqlConnectionString));

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider("/app/images"),
            //    RequestPath = "/images"
            //});

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
