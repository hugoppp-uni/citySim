using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Util;
using CitySim.Backend.World;
using Mars.Components.Services.Planning.ActionCommons;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents;

public class Person : IAgent<WorldLayer>, IPositionableEntity
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private WorldLayer _worldLayer = null!; //Init()

    public string Name { get; }
    private readonly PersonGoap _goap;

    public int Hunger { get; set; } = 50;
    public int Food { get; set; } = 30;
    public PathFindingRoute Route = PathFindingRoute.CompletedRoute;
    private PersonAction? _plannedAction;

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

    public void Init(WorldLayer layer)
    {
        _worldLayer = layer;
        Position = _worldLayer.RandomPosition();
        _worldLayer.GridEnvironment.Insert(this);
    }

    public void Tick()
    {
        if (!ApplyGameRules())
            //return value is false if the agent died, hacky for now
            return;

        var personsInVicinity = _worldLayer.GridEnvironment
            .Explore(Position, 2, predicate: person => person.ID != ID)
            .Select(p =>  p switch{ Person person => person.Name, _ => p.GetType().ToString()});
        Console.WriteLine(
            $"{Name} (Hunger: {Hunger}, Food: {Food}) {Position} can see: [{string.Join(',', personsInVicinity)}]");

        if (_plannedAction is null)
        {
            if (_goap.Plan().ToList().Any(action => action is not AllGoalsSatisfiedAction))
            {
                var goapAction = _goap.Plan().ToList().First();
                if (goapAction is PersonAction personAction)
                {
                    _plannedAction = personAction;
                    Route = _worldLayer.FindRoute(Position, _plannedAction.TargetPosition);
                }
            }
        }

        if (_plannedAction is not null)
        {
            if (1 > Distance.Euclidean(Position.PositionArray, _plannedAction.TargetPosition.PositionArray))
            {
                Console.WriteLine(_plannedAction.DescriptionVerb);
                _plannedAction.Execute();
                _plannedAction = null;
                return;
            }

            if (!Route.Completed)
            {
                Console.WriteLine($"Moving to {_plannedAction.TargetPosition} to " + _plannedAction.DescriptionNoun);
                (int x, int y) = Route.Next();
                _worldLayer.GridEnvironment.PosAt(this, x, y);
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
        _worldLayer.Kill(this);
    }

    public double Extent { get; set; } = 1;
}