using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using MOh.GRPC.Server.Protos;

namespace Moh.GRPC.Client
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly string devceId;
        private readonly TelmteryService.TelmteryServiceClient client;

        public Worker(ILogger<Worker> logger ,string devceId)
        {
            _logger = logger;
            this.devceId = devceId;

            var grpcChannel = GrpcChannel.ForAddress("https://localhost:5044");
            client = new TelmteryService.TelmteryServiceClient(grpcChannel);

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            Random rand = new Random();
            while (!stoppingToken.IsCancellationRequested)
            {
                await sendMessage(rand);

                // var stream = await client.keepAlive();
                await   KeepAlive(stoppingToken);



                 await Task.Delay(1000, stoppingToken);
            }
        }

        private async Task KeepAlive(CancellationToken stoppingToken)
        {
            var stream = client.keepAlive();

            var keepAliveTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await stream.RequestStream.WriteAsync(new PulseMessage
                    {
                        ClientStatus = ClientStatus.Working,
                        Details = $"Device Id: {devceId} is working!",
                        Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(2000);
                }
            });

            await keepAliveTask;
        }

        private async Task sendMessage(Random rand)
        {
            var dev = new TrackingMessage
            {
                DevicedId = int.Parse(devceId),
                Speed = 4,
                Location = new Location { Lang = rand.Next(0, 10), Lat = rand.Next(0, 10) },
                Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };
            dev.Sensor.Add(new Sensor { Key = "ss", Value = 6 });
            dev.Sensor.Add(new Sensor { Key = "sus", Value = 6 });
            var x = await client.sendMessageAsync(dev);
            _logger.LogInformation(x.Success.ToString());
        }
    }
}