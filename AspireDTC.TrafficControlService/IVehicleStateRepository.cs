using AspireDTC.TrafficControlService.Models;

namespace AspireDTC.TrafficControlService;

public interface IVehicleStateRepository
{
    Task SaveVehicleStateAsync(VehicleState vehicleState);
    Task<VehicleState?> GetVehicleStateAsync(string licenseNumber);
}
