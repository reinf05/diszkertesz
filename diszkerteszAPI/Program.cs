using Azure.Storage.Blobs;
using diszkerteszAPI.Controllers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Identity.Web;

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
            builder.Services.AddDbContext<diszkerteszDbContext>(options => options.UseAzureSql(sqlConnectionString));

            builder.Services.AddSingleton<BlobServiceClient>(x =>
            {
                var blobConnectionString = builder.Configuration.GetConnectionString("BlobConnection");
                return new BlobServiceClient(blobConnectionString);
            });

            builder.Services.AddControllers();
            builder.Services.AddHttpContextAccessor();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

            var app = builder.Build();

            app.UseAuthentication();

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

                app.Use(async (HttpContext context, Func<Task> next) =>
                {
                    // Check if the user is NOT authenticated (which happens locally)
                    if (!context.User.Identity?.IsAuthenticated ?? true)
                    {
                        var claims = new[]
                        {
                            new System.Security.Claims.Claim("http://schemas.microsoft.com/identity/claims/objectidentifier", "local-test-user-2"),
                            new System.Security.Claims.Claim("name", "Test Developer")
                        };

                        var identity = new System.Security.Claims.ClaimsIdentity(claims, "TestAuthType");
                        context.User = new System.Security.Claims.ClaimsPrincipal(identity);
                    }

                    // Invoke the next middleware
                    await next.Invoke();
                });
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
