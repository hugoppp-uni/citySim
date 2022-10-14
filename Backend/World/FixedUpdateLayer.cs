using System.Diagnostics;
using CitySim.Backend.Util;
using Mars.Components.Layers;
using Mars.Interfaces.Layers;
using NLog;

namespace CitySim.Backend.World;

public class FixedUpdateLayer : AbstractLayer, ISteppedActiveLayer
{
    internal SimulationController SimulationController = null!; // set in CitySim ctor
    private readonly Stopwatch _stopwatch = new();

    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public void Tick()
    {
    }

    public void PreTick()
    {
    }

    public void PostTick()
    {
        if (_stopwatch.IsRunning)
        {
            _logger.Debug(
                $"############################### Tick: {Context.CurrentTick} ############################### ");
            _logger.Debug($"Current ticks per second: {SimulationController.TicksPerSecond:F1}");
            long msToWait =
                Math.Max(0, Convert.ToInt64(GetCurrentTick() * SimulationController.MsPerTick - _stopwatch.ElapsedMilliseconds));
            _logger.Debug($"CPU %: {(1 - msToWait / SimulationController.MsPerTick)*100}");
            if (msToWait > 0)
            {
                _logger.Debug($"Waiting: {msToWait:F1}ms");
                Task.Delay(TimeSpan.FromMilliseconds(msToWait)).Wait();
            }
        }
        else
        {
            _stopwatch.Start();
        }
    }
}