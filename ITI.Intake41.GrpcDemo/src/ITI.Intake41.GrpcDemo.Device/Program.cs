using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITI.Intake41.GrpcDemo.Device
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Device Id");

            int deviceId = int.Parse(Console.ReadLine());

            CreateHostBuilder(deviceId, args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(int deviceId, string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<Worker>(provider =>
                    {
                        var logger = provider.GetService<ILogger<Worker>>();

                        return new Worker(logger, deviceId);
                    });
                });
    }
}
