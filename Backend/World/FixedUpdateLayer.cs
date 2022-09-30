using Mars.Components.Layers;
using Mars.Interfaces.Layers;

namespace CitySim.Backend.World;

public class FixedUpdateLayer : AbstractLayer, ISteppedActiveLayer
{
    public void Tick()
    {
        Task.Delay(TimeSpan.FromMilliseconds(1000)).Wait();
        //todo
    }

    public void PreTick()
    {
    }

    public void PostTick()
    {
    }
}