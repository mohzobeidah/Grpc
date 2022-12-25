using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.WellKnownTypes;
using Grpc.Net.Client;
using ITI.Intake41.GrpcDemo.Server.Protos;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ITI.Intake41.GrpcDemo.Device
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly int deviceId;
        private TrackingService.TrackingServiceClient _client;

        private TrackingService.TrackingServiceClient Client
        {
            get
            {
                if (_client == null)
                {
                    var channel = GrpcChannel.ForAddress("https://localhost:5001");

                    _client = new TrackingService.TrackingServiceClient(channel);
                }

                return _client;
            }
        }

        public Worker(ILogger<Worker> logger,  int deviceId)
        {
            _logger = logger;
            this.deviceId = deviceId;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Random random = new Random();

           var keepAliveTask = KeepAlive(stoppingToken);

            var subscribeTask = SubscribeNotification(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                await SendMessage(random);

                await Task.Delay(1000, stoppingToken);
            }

            await Task.WhenAll(keepAliveTask, subscribeTask);
        }

        private async Task SubscribeNotification(CancellationToken stoppingToken)
        {
            var responseStream = Client.SubscribeNotification(new SubscribeRequest { DeviceId = deviceId });

            var task = Task.Run(async () =>
            {
                while (await responseStream.ResponseStream.MoveNext(stoppingToken))
                {
                    var msg = responseStream.ResponseStream.Current;

                    _logger.LogInformation($"New Message Received Text: {msg.Text}");
                }
            });

            await task;
        }

        private async Task KeepAlive(CancellationToken stoppingToken)
        {
            var stream = Client.KeepAlive();

            var keepAliveTask = Task.Run(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await stream.RequestStream.WriteAsync(new PulseMessage
                    {
                        Status = ClientStatus.Working,
                        Details = $"Device Id: {deviceId} is working!",
                        Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(2000);
                }
            });

            await keepAliveTask;
        }

        private async Task SendMessage(Random random)
        {
            var request = new TrackingMessage
            {
                DeviceId = deviceId,
                Speed = random.Next(0, 220),
                Location = new Location { Lat = random.Next(0, 100), Long = random.Next(0, 100) },
                Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
            };

            request.Sensors.Add(new Sensor { Key = "temp", Value = 30 });
            request.Sensors.Add(new Sensor { Key = "door", Value = 1 });

            var response = await Client.SendMessageAsync(request);

            _logger.LogInformation($"Response: {response.Success}");
        }
    }
}
