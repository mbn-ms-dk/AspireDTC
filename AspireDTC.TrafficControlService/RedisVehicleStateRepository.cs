using AspireDTC.TrafficControlService.Models;
using StackExchange.Redis;
using System.Text.Json;

namespace AspireDTC.TrafficControlService;

public class RedisVehicleStateRepository(ILogger<RedisVehicleStateRepository> logger, IConnectionMultiplexer redis) : IVehicleStateRepository
{

    private readonly ILogger<RedisVehicleStateRepository> _logger = logger;
    private readonly IConnectionMultiplexer _redis = redis;
    private readonly IDatabase _database = redis.GetDatabase();
    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public async Task<VehicleState?> GetVehicleStateAsync(string licenseNumber)
    {
        var data = await _database.StringGetAsync(licenseNumber);

        if (data.IsNullOrEmpty)
        {
            return null;
        }

        return JsonSerializer.Deserialize<VehicleState>(data, _jsonSerializerOptions);
    }

    public async Task SaveVehicleStateAsync(VehicleState vehicleState)
    {
        var created = await _database.StringSetAsync(vehicleState.LicenseNumber, JsonSerializer.Serialize(vehicleState, _jsonSerializerOptions));

        if (!created)
        {
            _logger.LogError("Failed to save vehicle state for {LicenseNumber}", vehicleState.LicenseNumber);
        }
    
    }

    private IServer GetServer()
    {
        var endpoint = _redis.GetEndPoints();
        return _redis.GetServer(endpoint.First());
    }
}
