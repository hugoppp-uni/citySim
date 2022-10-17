namespace CitySim.Backend.Util;

public class SimulationController
{
    public int TicksPerSecond { get; set; }
    public decimal MsPerTick => 1000m / TicksPerSecond;
}