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
        var trafficControlEndpoint = "http://localhost:5258";
        _logger.LogInformation($"Using HTTP endpoint {trafficControlEndpoint}");
        var client = new HttpClient { BaseAddress = new Uri(trafficControlEndpoint) };

        var cameras = Enumerable.Range(0, 4).Select(cameraNumber => new CameraHttpSimulation(cameraNumber, client));

        while (!stoppingToken.IsCancellationRequested)
        {
            Parallel.ForEach(cameras, async camera => await camera.Start());

            await Task.Run(() => Thread.Sleep(Timeout.Infinite), stoppingToken).WaitAsync(stoppingToken);

            Task.Delay(5000, stoppingToken).Wait();
        }
    }
}
