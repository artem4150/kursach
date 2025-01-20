using kursach.Models;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace SharedData
{
    public static class ConfigurationHelper
    {
        public static IConfiguration GetConfiguration()
        {
            var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Указывает путь, где искать appsettings.json
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }
    }
}
//"C: /Users/artem/source/repos/kursach/kursach/appsettings.json"