using AspireDTC.VehicleRegistrationService;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddScoped<IVehicleInfoRepository, InMemoryVehicleInfoRepository>();

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
app.MapGet("vehicleinfo", (string licenseNumber, IVehicleInfoRepository repo) => {
    Console.WriteLine($"Retrieving vehicle-info for licensenumber {licenseNumber}");
    var info = repo.GetVehicleInfo(licenseNumber);
    return Results.Ok(info);
});

app.Run();


