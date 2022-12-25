using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using MOh.GRPC.Server.Protos;

namespace MOh.GRPC.Server.Services
{
    public class TelemtryService : TelmteryService.TelmteryServiceBase
    {
        private readonly ILogger<TelemtryService> logger;

        public TelemtryService(ILogger<TelemtryService> logger)
        {
            this.logger = logger;


        }
        public override Task<TrackingResponse> sendMessage(TrackingMessage request, ServerCallContext context)
        {

            logger.LogInformation($"DevicedId {request.DevicedId} Speed :{request.Speed} sensor count {request.Sensor.Count}");

            return Task.FromResult(new TrackingResponse { Success = true });
        }

        public override async Task<Empty> keepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
        {
            var task = Task.Run(async () =>
            {
                await foreach (var item in requestStream.ReadAllAsync())
                {
                    logger.LogInformation($"{nameof(keepAlive)} status {item.Details} status {item.ClientStatus}");
                }

            });

            await task;

            return new Empty();
        }
        public override async Task SubscribeNotifaction(SubscribeRequest request, IServerStreamWriter<Notifcation> responseStream, ServerCallContext context)
        {
            var task = Task.Run(async () =>
             {
                 while (!context.CancellationToken.IsCancellationRequested)
                 {
                     //logger.LogInformation($"{nameof(SubscribeNotifaction)} device {request.DevicedId}");
                     await responseStream.WriteAsync(new Notifcation { Text = $"SubscribeNotifaction {request.DevicedId}", Stamp = Timestamp.FromDateTime(DateTime.UtcNow) });
                     await Task.Delay(3000);
                 }
             });

            await task;

        }
    }
}
