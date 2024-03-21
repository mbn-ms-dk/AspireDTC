using AspireDTC.TrafficSimulationWorker.Events;
using Microsoft.Extensions.Logging;

namespace AspireDTC.TrafficSimulationWorker;

public class CameraSimulation
{
    private readonly ITrafficControlService trafficControlService;
    private Random rnd;
    private int camNumber;
    private int minEntryDelayInMS = 50;
    private int maxEntryDelayInMS = 5000;
    private int minExitDelayInS = 4;
    private int maxExitDelayInS = 10;

    private readonly ILogger logger;

    public CameraSimulation(int cameraNumber, ITrafficControlService trafficControlService, ILogger logger)
    {
        this.trafficControlService = trafficControlService;
        camNumber = cameraNumber;
        rnd = new Random();
        this.logger = logger;
    }
    //Start simulation
    public Task start()
    {
        Console.WriteLine($"Start camera {camNumber} simulation");
        logger.LogInformation($"Start camera {camNumber} simulation");
        while (true)
        {
            try
            {
                var entryDelay = TimeSpan.FromMilliseconds(rnd.Next(minEntryDelayInMS, maxEntryDelayInMS) + rnd.NextDouble());
                Task.Delay(entryDelay).Wait();

                Task.Run(async () => {
                    //simulate entry
                    var entryTimestamp = DateTime.Now;
                    var vehicleRegistered = new VehicleRegistered
                    {
                        Lane = camNumber,
                        LicenseNumber = GenerateRandomLicenseNumber(),
                        Timestamp = entryTimestamp
                    };
                    await trafficControlService.SendVehicleEntryAsync(vehicleRegistered);
                    Console.WriteLine($"Simulated ENTRY of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                    logger.LogInformation($"Simulated ENTRY of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");


                    // simulate exit
                    var exitDelay = TimeSpan.FromSeconds(rnd.Next(minExitDelayInS, maxExitDelayInS) + rnd.NextDouble());
                    Task.Delay(exitDelay).Wait();
                    vehicleRegistered.Timestamp = DateTime.Now;
                    vehicleRegistered.Lane = rnd.Next(1, 4);
                    await trafficControlService.SendVehicleExitAsync(vehicleRegistered);
                    Console.WriteLine($"Simulated EXIT of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                    logger.LogInformation($"Simulated EXIT of vehicle with license-number {vehicleRegistered.LicenseNumber} in lane {vehicleRegistered.Lane}");
                }).Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Camera {camNumber} error: {ex.Message}");
                logger.LogError(ex, $"Camera {camNumber} error: {ex.Message}");
            }
        }
    }

    #region Private helper methods

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

        #endregion
}
