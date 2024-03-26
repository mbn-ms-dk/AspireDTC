using AspireDTC.FineCollectionWorker;
using System.Threading;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddSingleton<IFineCalculator, HardCodedFineCalculator>();

builder.Services.AddHttpClient<VehicleRegistrationService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5236/");
});
builder.Services.AddHostedService<Worker>();

builder.AddRabbitMQ("fine-messaging");


var host = builder.Build();
host.Run();
