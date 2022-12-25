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
             
            return Task.FromResult( new TrackingResponse {Success=true });
        }

        public override async Task<Empty> keepAlive(IAsyncStreamReader<PulseMessage> requestStream, ServerCallContext context)
        {
            var task=Task.Run(async () => {
                await foreach (var item in  requestStream.ReadAllAsync())
                {
                    logger.LogInformation($"{nameof(keepAlive)} status {item.Details} status {item.ClientStatus}");
                }
          
            });

            await task;
            
            return new Empty(); 
        }
    }
}
