using AspireDTC.TrafficSimulation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire components.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// obtain logger instance from di
var logger = builder.Services.BuildServiceProvider().GetService<ILogger<Program>>();


var app = builder.Build();


logger.LogInformation("Using mosquitto");
logger.LogInformation("Setting number of lanes");
int lanes = 3;
CameraSimulation[] cameras = new CameraSimulation[lanes];
for (var i = 0; i < lanes; i++)
{
    var camNumber = i + 1;
    ITrafficControlService trafficControlService = await MqttTrafficControlService.CreateAsync(camNumber);
    cameras[i] = new CameraSimulation(camNumber, trafficControlService, logger);
}
Parallel.ForEach(cameras, cam => cam.start());

Task.Run(() => Thread.Sleep(Timeout.Infinite)).Wait();

// Explicitly call Flush() followed by sleep is required in console apps.
// This is to ensure that even if application terminates, telemetry is sent to the back-end.
Task.Delay(5000).Wait();


app.MapDefaultEndpoints();


app.Run();

