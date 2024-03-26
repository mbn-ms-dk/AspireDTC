using AspireDTC.TrafficSimulationWorker.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace AspireDtC.TrafficSimulationWorker;

public  class CameraHttpSimulation
{
    private readonly HttpClient _client;
    private Random _random;
    private int _cameraNumber;
    private int _minEntryDelayInMilliseconds = 50;
    private int _maxEntryDelayInMilliseconds = 5000;
    private int _minExitDelayInSeconds = 5;
    private int _maxExitDelayInSeconds = 50;

    public CameraHttpSimulation(int cameraNumber, HttpClient client)
    {
        _random = new Random();
        _cameraNumber = cameraNumber;
        _client = client;
    }

    public async Task Start()
    {
        Console.WriteLine($"Start camera {_cameraNumber} simulation.");

        while (true)
        {
            try
            {
                var entryDelay = TimeSpan.FromMilliseconds(_random.Next(_minEntryDelayInMilliseconds, _maxEntryDelayInMilliseconds) + _random.NextDouble());
                await Task.Delay(entryDelay);

                await Task.Run(async () =>
                {
                    // simulate entry
                    var entryTimestamp = DateTime.Now;
                    var vehicleRegistered = new VehicleRegistered
                    {
                        Lane = _cameraNumber,
                        LicenseNumber = GenerateRandomLicenseNumber(),
                        Timestamp = entryTimestamp
                    };
                    var entry = await _client.PostAsJsonAsync("entrycam", vehicleRegistered);
                    entry.EnsureSuccessStatusCode();

                    Console.WriteLine($"Simulated ENTRY of vehicle with license number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                    
                    // simulate exit
                    var exitDelay = TimeSpan.FromSeconds(_random.Next(_minExitDelayInSeconds, _maxExitDelayInSeconds) + _random.NextDouble());
                    await Task.Delay(exitDelay);
                    vehicleRegistered.Timestamp = DateTime.Now;
                    vehicleRegistered.Lane = _random.Next(1, 4);
                    var exit = await _client.PostAsJsonAsync("exitcam", vehicleRegistered);
                    exit.EnsureSuccessStatusCode();

                    Console.WriteLine($"Simulated EXIT of vehicle with license number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera {_cameraNumber} error: {ex.Message}");
            }
        }
    }


    private string GenerateRandomLicenseNumber()
    {
        // Define the format of Danish license plates (XX 11 222)

        // Generate two random letters
        var letters = new string(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZ", 2)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());

        // Generate two random digits
        var digits = new string(Enumerable.Repeat("0123456789", 2)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());

        // Generate three more random digits
        var moreDigits = new string(Enumerable.Repeat("0123456789", 3)
            .Select(s => s[new Random().Next(s.Length)]).ToArray());

        // Combine the parts to form the license plate
        var licensePlate = $"{letters} {digits} {moreDigits}";

        return licensePlate;
    }
}
