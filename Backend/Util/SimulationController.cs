using CitySim.Backend.Entity.Agents;

namespace CitySim.Backend.Util;

public class SimulationController
{
    private int _ticksPerSecond = 1;
    public int TicksPerSecond { get; set; }

    private bool _paused;
    public bool Paused
    {
        get => _paused;
        set
        {
            _paused = value;
            if (!value)
            {
                ContinueEvent.Set();
            }
        }
    }

    public decimal MsPerTick => 1000m / TicksPerSecond;
    public AutoResetEvent ContinueEvent = new(false);
}