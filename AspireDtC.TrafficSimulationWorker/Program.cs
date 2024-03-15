using AspireDtC.TrafficSimulationWorker;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddHostedService<Worker>();

var app = builder.Build();
app.MapDefaultEndpoints();
app.Run();
