
using DapperApp.Repository;
using System.Globalization;

namespace Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // detect invariant culture
            var invariantCulture = CultureInfo.GetCultureInfo("en-US").EnglishName.Contains("Invariant");
            if (invariantCulture)
            {
                Console.WriteLine("Invariant culture is on");
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddSingleton(builder.Configuration);
            builder.Services.AddScoped<CompanyContactsRepository>();
            builder.Services.AddScoped<CompanyRepository>();
            var app = builder.Build();

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
