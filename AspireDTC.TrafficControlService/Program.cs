using AspireDTC.TrafficControlService;
using AspireDTC.TrafficControlService.Events;
using AspireDTC.TrafficControlService.Models;
using RabbitMQ.Client;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddSingleton<ISpeedingViolationCalculator>(new DefaultSpeedingViolationCalculator("A12", 10, 110, 5));

builder.AddRedisOutputCache("trafficcache");

//Add repositories
builder.Services.AddTransient<IVehicleStateRepository, RedisVehicleStateRepository>();

builder.AddRabbitMQ("fine-messaging");

builder.Services.AddCors();

var app = builder.Build();


app.UseOutputCache();

app.MapDefaultEndpoints();



// Configure the HTTP request pipeline.

//Test endpoint for API (not used in this demo)
app.MapGet("/", () => "Hi from Api");

app.MapPost("entrycam", async (VehicleRegistered msg, IVehicleStateRepository repo) => {
    try
    {
        //log entry
        Console.WriteLine($"ENTRY detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
            $"of vehicle with licenseplate {msg.LicenseNumber}");
        //store vehicle state
        var vehicleState = new VehicleState(msg.LicenseNumber, msg.Timestamp, null);
        await repo.SaveVehicleStateAsync(vehicleState);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"ENTRY: {ex}");
        return Results.Problem(ex.Message, ex.StackTrace, 500);
    }
});

app.MapPost("exitcam", async (VehicleRegistered msg, IVehicleStateRepository repo, ISpeedingViolationCalculator calc, IConnection connection) => {
    try
    {
        var state = await repo.GetVehicleStateAsync(msg.LicenseNumber);
        if (state == default(VehicleState))
            return Results.NotFound(msg.LicenseNumber);

        //log exit
        Console.WriteLine($"EXIT detected in lane {msg.Lane} at {msg.Timestamp.ToString("hh:mm:ss")} " +
            $"of vehicle with licenseplate {msg.LicenseNumber}");

        //update vehicle state
        var exitState = state.Value with { ExitTimeStamp = msg.Timestamp };
        await repo.SaveVehicleStateAsync(exitState);

        // handle possible speeding violation
        int violation = calc.DetermineSpeedingViolationInKmh(exitState.EntryTimeStamp, exitState.ExitTimeStamp.Value);
        if (violation > 0)
        {
            Console.WriteLine($"Speeding violation detected ({violation} KMh) of vehicle" +
                $"with license-number {state.Value.LicenseNumber}.");

            var speedingViolation = new SpeedingViolation
            {
                VehicleId = msg.LicenseNumber,
                RoadId = calc.GetRoadId(),
                ViolationInKmh = violation,
                Timestamp = msg.Timestamp
            };

            // publish speedingviolation
            Console.WriteLine($"PUBLISHING {speedingViolation.VehicleId}");
            JsonSerializerOptions jsonSerializerOptions = new()
            {
                PropertyNameCaseInsensitive = true,
            };
            var queueName = "collectfines";
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            channel.BasicPublish("", queueName, null, JsonSerializer.SerializeToUtf8Bytes(speedingViolation, jsonSerializerOptions));
            Console.WriteLine($"PUBLISHED {speedingViolation.VehicleId}");
        }

        return Results.Ok();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"EXIT: {ex}");
        return Results.Problem(ex.Message, ex.StackTrace, 500);
    }
});

app.UseCors(cors =>
{
    cors.AllowAnyHeader();
    cors.AllowAnyMethod();
    cors.AllowAnyOrigin();
});

app.Run();

