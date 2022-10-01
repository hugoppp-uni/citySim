using CitySim.Backend.Agents.Behavior;
using CitySim.Backend.World;
using Mars.Common;
using Mars.Components.Services.Planning.ActionCommons;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace CitySim.Backend.Agents;

public class Person : IAgent<GridLayer>, IPositionable
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private GridLayer _gridLayer = null!; //Init()

    public string Name { get; }
    private readonly PersonGoap _goap;

    public int Hunger { get; set; } = 15;
    public int Food { get; set; } = 30;

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

        var goapActions = _goap.Plan().ToList();
        if (goapActions.Any(action => action is not AllGoalsSatisfiedAction))
        {
            var goapAction = goapActions.First();
            if (goapAction is PersonAction personAction)
            {
                if (0 == Distance.Chebyshev(Position.PositionArray, personAction.GetTargetPosition().PositionArray))
                {
                    Console.WriteLine("Exectuing: " + goapAction.GetType());
                    goapAction.Execute();
                }
                else
                {
                    Console.WriteLine($"Moving to {personAction.GetTargetPosition()} to execute: " + goapAction.GetType());
                    //todo seems like this is the wrong method for this, the person always move to bottom right
                    _gridLayer.GridEnvironment.MoveTo(this, personAction.GetTargetPosition(), 1);
                }
            }
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