using AspireDTC.TrafficSimulation.Events;

namespace AspireDTC.TrafficSimulation;

public interface ITrafficControlService
{
    public Task SendVehicleEntryAsync(VehicleRegistered vehicleRegistered);
    public Task SendVehicleExitAsync(VehicleRegistered vehicleRegistered);
}
