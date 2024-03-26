using AspireDTC.FineCollectionWorker.Models;
using System.Net.Http.Json;

namespace AspireDTC.FineCollectionWorker;

public class VehicleRegistrationService(HttpClient httpClient)
{
     public async Task<VehicleInfo> GetVehicleInfoAsync(string licenseNumber)
    {
        Console.WriteLine("Retrieving vehicle-info for licensenumber {licenseNumber}");
        return await httpClient.GetFromJsonAsync<VehicleInfo>($"vehicleinfo?licensenumber={licenseNumber}");
    }
}
