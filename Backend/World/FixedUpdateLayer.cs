using Mars.Components.Layers;
using Mars.Interfaces.Layers;

namespace CitySim.Backend.World;

public class FixedUpdateLayer : AbstractLayer, ISteppedActiveLayer
{
    internal SimulationController SimulationController = null!; // set in CitySim ctor
    public void Tick()
    {
        Task.Delay(TimeSpan.FromMilliseconds(1000 / SimulationController.TimeMultiplier)).Wait();
        //todo
    }

    public void PreTick()
    {
    }

    public void PostTick()
    {
    }
}