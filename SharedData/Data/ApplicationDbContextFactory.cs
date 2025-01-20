using kursach.Data;
using kursach.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SharedData.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Явный путь к файлу appsettings.json
            var pathToAppSettings = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");


            // Создаем конфигурацию с указанием пути
            var configuration = new ConfigurationBuilder()
                .AddJsonFile(pathToAppSettings)
                .Build();

            // Настраиваем DbContext
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
