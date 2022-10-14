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
        _logger.Debug(
            $"############################### Starting Tick: {Context.CurrentTick} ###############################");
    }

    public void PostTick()
    {
        if (_stopwatch.IsRunning)
        {
            _logger.Debug(
                $"############################### Finished Tick: {Context.CurrentTick} ############################### ");
            _logger.Debug($"Current ticks per second: {SimulationController.TicksPerSecond:F1}");
            long timeToWait =
                Convert.ToInt64(GetCurrentTick() * SimulationController.TimePerTick - _stopwatch.ElapsedMilliseconds);
            if (timeToWait > 0)
            {
                _logger.Debug($"Waiting: {timeToWait:F1}ms");
                Task.Delay(TimeSpan.FromMilliseconds(timeToWait)).Wait();
            }
        }
        else
        {
            _stopwatch.Start();
        }
    }
}