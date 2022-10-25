using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using CitySim.Backend.World;
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

    private readonly IMind _mind;
    private readonly PersonRecollection _recollection = new();

    public PersonNeeds Needs { get; } = new();

    public PathFindingRoute Route = PathFindingRoute.CompletedRoute;
    private PersonAction? _plannedAction;


    public Person()
    {
        _mind = MindMock.Instance;
    }

    public void Init(WorldLayer layer)
    {
        _worldLayer = layer;
        Position = _worldLayer.RandomPosition();
        _worldLayer.GridEnvironment.Insert(this);

        lock (_worldLayer.Structures)
        {
            var home = _worldLayer.Structures.OfType<House>().First(house => house.FreeSpaces > 0);
            home.FreeSpaces--;
            _recollection.Add(ActionType.Sleep, home.Position);
        }
        _recollection.Add(ActionType.Eat , _worldLayer.Structures.OfType<Restaurant>().First().Position);
    }

    public void Tick()
    {
        if (!ApplyGameRules())
            //return value is false if the agent died, hacky for now
            return;

        var personsInVicinity = _worldLayer.GridEnvironment
            .Explore(Position, 2, predicate: person => person.ID != ID);

        if (_plannedAction is null)
        {
            _plannedAction = PlanNextAction();
            if (_plannedAction is null) return;
            Route = _worldLayer.FindRoute(Position, _plannedAction.TargetPosition);
        }


        if (!Route.Completed)
        {
            _logger.Trace($"Moving to {_plannedAction.TargetPosition} to " + _plannedAction);
            (int x, int y) = Route.Next();
            _worldLayer.GridEnvironment.PosAt(this, x, y);
            return;
        }

        _logger.Trace($"{ID} is {_plannedAction}");
        if (_plannedAction.Execute() == ActionResult.Executed)
            _plannedAction = null;
    }

    private PersonAction PlanNextAction()
    {
        var nextActionType = _mind.GetNextActionType(Needs, _worldLayer.GetGlobalState());
        Position? nearestActionPos = _recollection.ResolvePosition(nextActionType)
            .MinBy(position => Distance.Manhattan(position.PositionArray, Position.PositionArray));
        if (nearestActionPos != null)
        {
            return new PersonAction(nextActionType, nearestActionPos, this);
        }
        else
        {
            //todo exploration
            return null;
        }
    }

    private bool ApplyGameRules()
    {
        Needs.Tick();
        if (Needs.Hunger < 0)
        {
            Kill();
            _logger.Trace($"{ID} DIED of starvation");
            return false;
        }

        return true;
    }

    private void Kill()
    {
        _worldLayer.Kill(this);
    }
}