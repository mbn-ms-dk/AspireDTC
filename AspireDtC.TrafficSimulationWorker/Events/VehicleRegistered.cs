namespace AspireDTC.TrafficSimulationWorker.Events;

//create a new record struct with lane, licensenumber, and timestamp
public record struct VehicleRegistered(int Lane, string LicenseNumber, DateTime Timestamp);
