using Microsoft.Extensions.Hosting;
using Moh.GRPC.Client;

Console.WriteLine("Device Id :");
var devceId =Console.ReadLine();


IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>(e =>
        {
           var logger= e.GetRequiredService<ILogger<Worker>>();
            var worker = new Worker(logger, devceId);

            return worker;

        });
    })
    .Build();

host.Run();
