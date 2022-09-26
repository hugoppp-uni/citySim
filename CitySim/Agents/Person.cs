namespace CitySim.Agents;
using System;
using Mars.Interfaces.Agents;
using World;

public class Person: IAgent<GridWorld>
{
    public void Init(GridWorld layer)
    {
        Console.WriteLine("Agent init");
    }

    public void Tick()
    {
        Console.WriteLine("Agent ticks");
    }

    public Guid ID { get; set; }
}
