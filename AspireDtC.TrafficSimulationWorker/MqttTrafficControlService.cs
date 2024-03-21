using AspireDTC.TrafficSimulationWorker.Events;
using MQTTnet;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AspireDTC.TrafficSimulationWorker;

public  class MqttTrafficControlService : ITrafficControlService
{
    private IMqttClient client;

    public MqttTrafficControlService(IMqttClient client)
    {
        this.client = client;
    }

    public static async Task<MqttTrafficControlService> CreateAsync(int cameraNumber)
    {
        var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST") ?? "localhost:1883";
        var factory = new MqttFactory();
        var mqttClient = factory.CreateMqttClient();
        var uri = new Uri(mqttHost);
        var options = new MqttClientOptionsBuilder()
            .WithTcpServer(uri.Host, uri.Port)
            .WithClientId($"camerasimulation{cameraNumber}")
            .Build();

        try
        {
            await mqttClient.ConnectAsync(options, CancellationToken.None);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        return new MqttTrafficControlService(mqttClient);
    }

    public async Task SendVehicleEntryAsync(VehicleRegistered vehicleRegistered)
    {
        var evt = JsonSerializer.Serialize(vehicleRegistered);
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic("trafficcontrol/entrycam")
            .WithPayload(Encoding.UTF8.GetBytes(evt))
            .Build();
        await client.PublishAsync(msg, CancellationToken.None);
    }

    public async Task SendVehicleExitAsync(VehicleRegistered vehicleRegistered)
    {
        var evt = JsonSerializer.Serialize(vehicleRegistered);
        var msg = new MqttApplicationMessageBuilder()
            .WithTopic("trafficcontrol/exitcam")
            .WithPayload(Encoding.UTF8.GetBytes(evt))
            .Build();
        await client.PublishAsync(msg, CancellationToken.None);
    }
}
