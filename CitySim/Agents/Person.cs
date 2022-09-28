using CitySim.World;
using Mars.Common;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;

namespace CitySim.Agents;

public class Person : IAgent<GridLayer>, IPositionable
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()

    private GridLayer _gridLayer = null!; //Init()

    public string Name { get; }

    [PropertyDescription]
    public long Hunger { get; set; }

    /// <summary>
    /// Whatever the person has to say at the moment.
    /// </summary>
    public string Comment { get; set; } = "";

    public bool Alive { get; private set; } = true;

    private long _movingToFarm = 0;

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
        Comment = "\""; // Surround current comment with quotes to allow mulitple lines
        Hunger++;
        
        See();

        var centerBearing = Position.GetBearing(_gridLayer.GridEnvironment.Centre);
        _gridLayer.GridEnvironment.MoveTowards(this, centerBearing, distanceToPass: 1);
        
        React();
        Comment += "\"";
    }

    private void See()
    {
        var personsInVicinity = _gridLayer.GridEnvironment.Explore(Position, radius: 2)
            .Where(p => p.ID != ID)
            .Select(p => p.Name);
        Console.WriteLine($"{Name} {Position} can see: [{string.Join(',', personsInVicinity)}]");
    }

    private void React()
    {
        if (Hunger >= 10)
        {
            Alive = false;
            _gridLayer.RemoveAgent(this);
            Console.WriteLine($"{Name} died of hunger");
            Comment += "Died of hunger";
            return;
        }

        if (_movingToFarm > 0)
        {
            _movingToFarm--;
            if (_movingToFarm == 0)
            {
                Comment += "Got to eat!";
                Hunger = 0;
            }
        }
        
        if (Hunger > 3 && _movingToFarm == 0)
        {
            Comment += "Time to get something to eat..." + Environment.NewLine;
            _movingToFarm = Random.Shared.Next(8) + 1; // 1-8 moves required to get to farm
        }
    }
}
