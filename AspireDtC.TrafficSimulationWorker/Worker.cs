using AspireDTC.TrafficSimulationWorker;
using Microsoft.AspNetCore.Builder;

namespace AspireDtC.TrafficSimulationWorker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Using mosquitto");
        _logger.LogInformation("Setting number of lanes");
        int lanes = 3;
        CameraSimulation[] cameras = new CameraSimulation[lanes];
        for (var i = 0; i < lanes; i++)
        {
            var camNumber = i + 1;
            ITrafficControlService trafficControlService = await MqttTrafficControlService.CreateAsync(camNumber);
            cameras[i] = new CameraSimulation(camNumber, trafficControlService, _logger);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            Parallel.ForEach(cameras, cam => cam.start());

            await Task.Run(() => Thread.Sleep(Timeout.Infinite), stoppingToken).WaitAsync(stoppingToken);

            Task.Delay(5000, stoppingToken).Wait();
        }
    }
}
