using Mars.Components.Layers;
using Mars.Interfaces.Layers;

namespace CitySim.Backend.World;

public class FixedUpdateLayer : AbstractLayer, ISteppedActiveLayer
{
    internal SimulationController SimulationController = null!; // set in CitySim ctor
    public void Tick()
    {
    }

    public void PreTick()
    {
        Console.WriteLine($"### Starting Tick: {Context.CurrentTick} ###");
    }

    public void PostTick()
    {
        Console.WriteLine($"### Finished Tick: {Context.CurrentTick} ###");
        Console.WriteLine($"Current simulation speed multiplier: {SimulationController.TimeMultiplier:F1}");
        
        //todo calculate
        var timeToWait = 1000 / SimulationController.TimeMultiplier;
        Console.WriteLine($"Waiting: {timeToWait:F1}ms");
        Task.Delay(TimeSpan.FromMilliseconds(timeToWait)).Wait();
    }
}