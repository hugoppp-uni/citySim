using CitySim.Backend.Agents.Behavior;
using CitySim.Backend.World;
using Mars.Common;
using Mars.Components.Services.Planning.ActionCommons;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents;

public class Person : IAgent<GridLayer>, IPositionable
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private GridLayer _gridLayer = null!; //Init()

    public string Name { get; }
    private readonly PersonGoap _goap;

    public int Hunger { get; set; } = 10;
    public int Food { get; set; } = Random.Shared.Next(0, 3);

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
        _goap = new(this);
    }

    public void Init(GridLayer layer)
    {
        _gridLayer = layer;
        Position = _gridLayer.RandomPosition();
        _gridLayer.GridEnvironment.Insert(this);
    }

    public void Tick()
    {
        if (!ApplyGameRules())
            //return value is false if the agent died, hacky for now
            return;

        var personsInVicinity = _gridLayer.GridEnvironment.Explore(Position, radius: 2)
            .Where(p => p.ID != ID)
            .Select(p => p.Name);
        Console.WriteLine(
            $"{Name} (Hunger: {Hunger}, Food: {Food}) {Position} can see: [{string.Join(',', personsInVicinity)}]");

        var centerBearing = Position.GetBearing(_gridLayer.GridEnvironment.Centre);
        _gridLayer.GridEnvironment.MoveTowards(this, centerBearing, distanceToPass: 1);

        var goapActions = _goap.Plan().ToList();
        if (goapActions.Any(action => action is not AllGoalsSatisfiedAction))
        {
            goapActions.First().Execute();
        }
    }

    private bool ApplyGameRules()
    {
        Hunger--;
        _goap.Tick();

        if (Hunger < 0)
        {
            Kill();
            Console.WriteLine($"{Name} DIED of starvation");
            return false;
        }

        return true;
    }

    private void Kill()
    {
        _gridLayer.GridEnvironment.Remove(this);
        _gridLayer.UnregisterAgent(_gridLayer, this);
    }
}