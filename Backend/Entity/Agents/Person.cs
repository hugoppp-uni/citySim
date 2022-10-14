using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Util;
using CitySim.Backend.World;
using Mars.Components.Services.Planning.ActionCommons;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NLog;

namespace CitySim.Backend.Entity.Agents;

public class Person : IAgent<WorldLayer>, IPositionableEntity
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private WorldLayer _worldLayer = null!; //Init()
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

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
        Names.TryDequeue(out var name);
        Name = name ?? ":(";
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
            .Select(p => p switch { Person person => person.Name, _ => p.GetType().ToString() });
        _logger.Trace(
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
                _logger.Trace($"{Name} is {_plannedAction.DescriptionVerb}");
                _plannedAction.Execute();
                _plannedAction = null;
                return;
            }

            if (!Route.Completed)
            {
                _logger.Trace($"Moving to {_plannedAction.TargetPosition} to " + _plannedAction.DescriptionNoun);
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
            _logger.Trace($"{Name} DIED of starvation");
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