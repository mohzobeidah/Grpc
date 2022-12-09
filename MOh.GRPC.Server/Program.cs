using MOh.GRPC.Server.Services;

var builder = WebApplication.CreateBuilder(args);
var enviro = builder.Environment;
builder.Services.AddLogging();
builder.Services.AddGrpcReflection();
builder.Services.AddGrpc();

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();
   
app.UseEndpoints(end =>
{
    //if (enviro.IsDevelopment())
    end.MapGrpcService<TelemtryService>();
        end.MapGrpcReflectionService();

});

app.Run();
