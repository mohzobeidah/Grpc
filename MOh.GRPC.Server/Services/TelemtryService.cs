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
    }
}
