using AspireDTC.TrafficSimulationWorker.Events;

namespace AspireDTC.TrafficSimulationWorker;

public interface ITrafficControlService
{
    public Task SendVehicleEntryAsync(VehicleRegistered vehicleRegistered);
    public Task SendVehicleExitAsync(VehicleRegistered vehicleRegistered);
}
