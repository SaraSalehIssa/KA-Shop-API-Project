
using KASHOP.DAL;
using KASHOP.DAL.Data;
<<<<<<< HEAD
using Microsoft.EntityFrameworkCore;
=======
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Globalization;
>>>>>>> 0659c09 (Localization)

namespace KASHOP.PL
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

<<<<<<< HEAD
=======
            builder.Services.AddLocalization(options => options.ResourcesPath = "");

>>>>>>> 0659c09 (Localization)
            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseSqlServer(builder.Configuration["ConnectionStrings: DefaultConnection"]));

            // OR

            //builder.Services.AddDbContext<ApplicationDbContext>(options =>
            //options.UseSqlServer(builder.Configuration.GetSection("ConnectionStrings")["DefaultConnection"]));
<<<<<<< HEAD
            
=======

>>>>>>> 0659c09 (Localization)
            // OR

            builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

<<<<<<< HEAD
            var app = builder.Build();

=======
            const string defaultCulture = "en";
            var supportedCultures = new[]
            {
                new CultureInfo(defaultCulture),
                new CultureInfo("ar"), // Add Arabic
            };
            builder.Services.Configure<RequestLocalizationOptions>(options => {
                options.DefaultRequestCulture = new RequestCulture(defaultCulture);
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
                options.RequestCultureProviders.Clear();
                options.RequestCultureProviders.Add(new QueryStringRequestCultureProvider
                {
                    QueryStringKey = "lang"
                });
            });

            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);

>>>>>>> 0659c09 (Localization)
            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
<<<<<<< HEAD
=======
                app.UseSwagger();
                app.UseSwaggerUI();
>>>>>>> 0659c09 (Localization)
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
