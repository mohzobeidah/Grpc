using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using ITI.Intake41.GrpcDemo.Server.Protos;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITI.Intake41.GrpcDemo.Server.Services
{
    public class TelemetryTrackingService : TrackingService.TrackingServiceBase
    {
        private readonly ILogger<TelemetryTrackingService> logger;

        public TelemetryTrackingService(ILogger<TelemetryTrackingService> logger)
        {
            this.logger = logger;
        }

        public override Task<TrackingResponse> SendMessage(TrackingMessage request, ServerCallContext context)
        {
            logger.LogInformation($"New Message: DeviceId: {request.DeviceId} " +
                $"Location: ({request.Location.Lat},{request.Location.Long}) Speed: {request.Speed} Sensors: {request.Sensors.Count}");

            return Task.FromResult(new TrackingResponse { Success = true });
        }

        public override  async Task<Empty> KeepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
        {
            var task =Task.Run(async () =>
            {
                await foreach (var item in requestStream.ReadAllAsync())
                {
                    logger.LogInformation($"{nameof(KeepAlive)}: Status: {item.Status} Details: {item.Details}");
                }
            });

            await task;

            return new Empty();
        }

        public override async Task SubscribeNotification(SubscribeRequest request, IServerStreamWriter<Notification> responseStream, ServerCallContext context)
        {
            var task = Task.Run(async () =>
            {
                while (!context.CancellationToken.IsCancellationRequested)
                {
                    await responseStream.WriteAsync(new Notification
                    {
                        Text = $"New Notification for Device {request.DeviceId}",
                        Stamp = Timestamp.FromDateTime(DateTime.UtcNow)
                    });

                    await Task.Delay(3000);
                }
            });

            await task;
        }
    }
}
