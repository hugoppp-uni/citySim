using CitySim.Backend.World;
using Mars.Common;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents;

public class Person : IAgent<GridLayer>, IPositionable
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()

    private GridLayer _gridLayer = null!; //Init()

    public string Name { get; }

    private static Queue<string> Names = new(new[]
    {
        "Peter",
        "Bob",
        "Micheal",
        "Gunther"
    });

    public Person()
    {
        Name = Names.Dequeue();
    }

    public void Init(GridLayer layer)
    {
        _gridLayer = layer;
        Position = _gridLayer.RandomPosition();
        _gridLayer.GridEnvironment.Insert(this);
        Console.WriteLine($"Agent {ID} init");
    }

    public void Tick()
    {
        var personsInVicinity = _gridLayer.GridEnvironment.Explore(Position, radius: 2)
            .Where(p => p.ID != ID)
            .Select(p => p.Name);
        Console.WriteLine($"{Name} {Position} can see: [{string.Join(',', personsInVicinity)}]");

        var centerBearing = Position.GetBearing(_gridLayer.GridEnvironment.Centre);
        _gridLayer.GridEnvironment.MoveTowards(this, centerBearing, distanceToPass: 1);
    }
}