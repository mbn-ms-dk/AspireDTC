using AspireDTC.FineCollectionWorker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;

namespace AspireDTC.FineCollectionWorker;

public class Worker : BackgroundService
{
    private IConnection connection;
    private VehicleRegistrationService vehicleRegistration;
    private IFineCalculator calculator;
    private IModel channel;

    public Worker(IConnection connection, VehicleRegistrationService vehicleRegistration, IFineCalculator calculator)
    {
        this.connection = connection;
        this.vehicleRegistration = vehicleRegistration;
        this.calculator = calculator;

        var queueName = "collectfines";

        channel = connection.CreateModel();
        channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);
        
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var queueName = "collectfines";
        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (chl, evt) =>
        {
            var content = Encoding.UTF8.GetString(evt.Body.ToArray());
            var speedingViolation = JsonSerializer.Deserialize<SpeedingViolation>(content);
            HandleSpeedingViolation(speedingViolation);
            channel.BasicAck(evt.DeliveryTag, false);
        };
        channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
    }

    public async Task HandleSpeedingViolation(SpeedingViolation speedingViolation)
    {

        //get owner information 
        var vehicleInfo = await vehicleRegistration.GetVehicleInfoAsync(speedingViolation.VehicleId);
        //log fine
        decimal fine = calculator.CalculateFine(speedingViolation.ViolationInKmh);
        var fineString = fine == 0 ? "tbd by the prosecutor" : $"{fine} Euro";
        Console.WriteLine($"Sent speeding ticket to {vehicleInfo.OwnerName}. " +
                $"Road: {speedingViolation.RoadId}, Licensenumber: {speedingViolation.VehicleId}, " +
                $"Vehicle: {vehicleInfo.Brand} {vehicleInfo.Model}, " +
                $"Violation: {speedingViolation.ViolationInKmh} Km/h, Fine: {fineString}, " +
                $"On: {speedingViolation.Timestamp.ToString("dd-MM-yyyy")} " +
                $"at {speedingViolation.Timestamp.ToString("hh:mm:ss")}.");
    }
}
