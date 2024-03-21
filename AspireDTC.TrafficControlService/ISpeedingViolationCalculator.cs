namespace AspireDTC.TrafficControlService;

public interface ISpeedingViolationCalculator
{
    int DetermineSpeedingViolationInKmh(DateTime entryTime, DateTime exitTime);
    string GetRoadId();
}

