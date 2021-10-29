using DAL;
using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Service;
using System.Threading.Tasks;

namespace Cloud_databases
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(Configure)
                .ConfigureOpenApi()
                .Build();

            host.Run();
        }

        static void Configure(HostBuilderContext Builder, IServiceCollection Services)
        {            
            Services.AddDbContext<CloudDBContext>();            
            Services.AddScoped<IMortgageService, MortgageService>();
            Services.AddScoped<IHouseService, HouseService>();
            Services.AddScoped<IMailService, MailService>();
        }
    }
}