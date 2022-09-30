namespace CitySim.Backend;

public class SimulationController
{
    private int _ticksPerSecond = 1;
    public int TicksPerSecond
    {
        get => _ticksPerSecond;
        set { _ticksPerSecond = value;
            TimePerTick = 1000m / value;
        }

    }

    public decimal TimePerTick { get; private set; } = 16.666666666666m;
}