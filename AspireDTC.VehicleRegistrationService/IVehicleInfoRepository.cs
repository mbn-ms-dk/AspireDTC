using AspireDTC.VehicleRegistrationService.Models;

namespace AspireDTC.VehicleRegistrationService;

public interface IVehicleInfoRepository
{
    VehicleInfo GetVehicleInfo(string licenseNumber);
}
