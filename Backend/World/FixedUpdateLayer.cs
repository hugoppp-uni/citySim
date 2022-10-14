using System.Diagnostics;
using CitySim.Backend.Util;
using Mars.Components.Layers;
using Mars.Interfaces.Layers;

namespace CitySim.Backend.World;

public class FixedUpdateLayer : AbstractLayer, ISteppedActiveLayer
{
    internal SimulationController SimulationController = null!; // set in CitySim ctor
    private readonly Stopwatch _stopwatch = new();

    public void Tick()
    {
    }

    public void PreTick()
    {
        Console.WriteLine( $"############################### Starting Tick: {Context.CurrentTick} ###############################");
    }

    public void PostTick()
    {
        if (_stopwatch.IsRunning)
        {
            Console.WriteLine( $"############################### Finished Tick: {Context.CurrentTick} ############################### ");
            Console.WriteLine($"Current ticks per second: {SimulationController.TicksPerSecond:F1}");
            long timeToWait =
                Convert.ToInt64(GetCurrentTick() * SimulationController.TimePerTick - _stopwatch.ElapsedMilliseconds);
            if (timeToWait > 0)
            {
                Console.WriteLine($"Waiting: {timeToWait:F1}ms");
                Task.Delay(TimeSpan.FromMilliseconds(timeToWait)).Wait();
            }
        }
        else
        {
            _stopwatch.Start();
        }
    }
}