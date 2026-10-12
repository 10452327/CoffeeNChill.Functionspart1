using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace CoffeeNChill.Functions
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .ConfigureServices(services =>
                {
                   
                })
                .Build();

            host.Run();
        }
    }
}