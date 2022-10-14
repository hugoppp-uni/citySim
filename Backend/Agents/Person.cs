using CitySim.Backend.Agents.Behavior;
using CitySim.Backend.World;
using Mars.Common;
using Mars.Components.Environments.Cartesian;
using Mars.Components.Services.Planning.ActionCommons;
using Mars.Interfaces.Agents;
using Mars.Numerics;
using NetTopologySuite.Geometries;
using Position = Mars.Interfaces.Environments.Position;

namespace CitySim.Backend.Agents;

public class Person : IAgent<GridLayer>, ICharacter
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private GridLayer _worldLayer = null!; //Init()

    public string Name { get; }
    private readonly PersonGoap _goap;

    public int Hunger { get; set; } = 50;
    public int Food { get; set; } = 30;
    public List<Position> Route = new();
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

    public void Init(GridLayer layer)
    {
        _worldLayer = layer;
        Position = _worldLayer.RandomPosition();
        _worldLayer.CollisionEnvironment.Insert(this, this.Position);
    }

    public void Tick()
    {
        if (!ApplyGameRules())
            //return value is false if the agent died, hacky for now
            return;

        var personsInVicinity = _worldLayer.CollisionEnvironment
            .ExploreCharacters(this, CreateExplorationWindow())
            .Select(p => p.Name);
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
                    Route = _worldLayer.CollisionEnvironment.FindRoute(this, personAction.TargetPosition)
                        .ToList();
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
                Route.Clear();
                return;
            }

            if (Route.Any())
            {
                Console.WriteLine($"Moving to {_plannedAction.TargetPosition} to " + _plannedAction.DescriptionNoun);
                MoveAlongRoute();
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

    public Polygon CreateExplorationWindow()
    {
        return new Polygon(new LinearRing(new[]
        {
            new Coordinate(Position.X - VisualRange, Position.Y - VisualRange),
            new Coordinate(Position.X - VisualRange, Position.Y + VisualRange),
            new Coordinate(Position.X + VisualRange, Position.Y + VisualRange),
            new Coordinate(Position.X + VisualRange, Position.Y - VisualRange),
            new Coordinate(Position.X - VisualRange, Position.Y - VisualRange)
        }));
    }

    public double VisualRange { get; set; } = 3;

    private void Kill()
    {
        _worldLayer.Kill(this);
    }

    public CollisionKind? HandleCollision(ICharacter other)
    {
        throw new NotImplementedException();
    }

    //https://git.haw-hamburg.de/mars/mars-learning/-/blob/main/Examples/Summer2022%20-%20Student%20Models/q-learning-goap-stronghold/Stronghold/Model/Agent/MovingAgent.cs#L133
    private void MoveAlongRoute()
    {
        Position position = Route[0];
        var distance = Distance.Chebyshev(Position.PositionArray, position.PositionArray);
        var direction = PositionHelper.CalculateBearingCartesian(Position.X, Position.Y, position.X, position.Y);
        if (distance > 0)
        {
            if (distance < 2)
                _worldLayer.CollisionEnvironment.PosAt(this, position);
            else
                _worldLayer.CollisionEnvironment.Move(this, direction, Math.Min(distance - 0.6, 1.0));
            return;
        }

        Route.RemoveAt(0);
    }


    public double Extent { get; set; } = 1;
}