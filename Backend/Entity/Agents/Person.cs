using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using CitySim.Backend.Util.Learning;
using CitySim.Backend.World;
using Mars.Components.Services;
using Mars.Core.Data;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NesScripts.Controls.PathFind;
using NLog;
using NLog.Fluent;
using CircularBuffer;

namespace CitySim.Backend.Entity.Agents;

public class Person : IAgent<WorldLayer>, IPositionableEntity
{
    public Guid ID { get; set; }
    private const int ReproductionRate = 2;
    public Position Position { get; set; } = null!; //Init()
    private WorldLayer _worldLayer = null!; //Init()
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private IMind _mind = null!;
    private readonly PersonRecollection _recollection = new();
    private readonly List<Action> _onKill = new();

    public PersonNeeds Needs { get; } = new();

    public record struct PersonActionLog(PersonNeeds Needs, PersonAction Action);
    

    private readonly CircularBuffer<PersonActionLog> _actionLog = new(20);
    public int GetActionLog(PersonActionLog[] ary) => _actionLog.WriteToArray(ary);

    public readonly string Name;
    public PathFindingRoute Route = PathFindingRoute.CompletedRoute;
    private PersonAction? _plannedAction;
    [PropertyDescription] public string ModelWorkerKey { get; set; }

    private int _tickAge = 0;

    public Person()
    {
        Name = WorldLayer.Instance.Names.GetRandom();
    }

    public void Init(WorldLayer layer)
    {
        _mind = new PersonMind(0.5, ModelWorker.GetInstance(ModelWorkerKey));
        _worldLayer = layer;
        Position = _worldLayer.RandomBuildingPosition();
        _worldLayer.GridEnvironment.Insert(this);

        _recollection.Add(ActionType.Eat, _worldLayer.Structures.OfType<Restaurant>().First().Position);
    }

    public ActionType? GetNextAction()
    {
        return _plannedAction?.Type;
    }

    public void Tick()
    {
        try
        {
            TickInternal();
        }
        catch (Exception e)
        {
            _logger.Fatal("Agent crashed:" + Environment.NewLine + "{e}", e);
            throw;
        }
    }

    private void TickInternal()
    {
        _tickAge++;
        if (!ApplyGameRules())
            //return value is false if the agent died, hacky for now
            return;

        if (_plannedAction is null)
        {
            _plannedAction = PlanNextAction();
            if (_plannedAction is null) return;
            
            _actionLog.PushFront(new PersonActionLog {Action = _plannedAction, Needs = Needs with {}});
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

    private PersonAction? PlanNextAction()
    {
        var nextActionType = _mind.GetNextActionType(
            Needs,
            _worldLayer.GetGlobalState(),
            new Distances(this, _worldLayer)
        );

        Position? GetPosition() => nextActionType switch
        {
            ActionType.BuildHouse => _worldLayer.BuildPositionEvaluator.GetNextBuildPos(),
            _ => _recollection.ResolvePosition(nextActionType)
                .MinBy(position => Distance.Manhattan(position.PositionArray, Position.PositionArray))
        };


        var actionPosition = GetPosition();
        if (actionPosition != null)
        {
            return new PersonAction(nextActionType, actionPosition, this);
        }

        if (nextActionType == ActionType.Sleep)
        {
            lock (_worldLayer.Structures)
            {
                var home = _worldLayer.Structures.OfType<House>().OrderBy(it =>
                        _worldLayer.FindRoute(it.Position, Position).Remaining)
                    .FirstOrDefault(house => house.FreeSpaces > 0);

                if (home is null)
                    return null;

                home.AddInhabitant(this);
                _onKill.Add(() => home.RemoveInhabitant(this));
                _recollection.Add(ActionType.Sleep, home.Position);
                return new PersonAction(ActionType.Sleep, home.Position, this);
            }
        }

        return null;
    }

    private bool ApplyGameRules()
    {
        Needs.Tick();
        if (Needs.Hunger < 0)
        {
            _mind.LearnFromDeath(ActionType.Eat);
            Kill();
            WorldLayer.Instance.EventLog.Log($"DIED of starvation", this);
            return false;
        }

        if (Needs.Sleepiness < -3)
        {
            _mind.LearnFromDeath(ActionType.Sleep);
            Kill();
            WorldLayer.Instance.EventLog.Log($"DIED of sleepiness", this);
            return false;
        }

        ReproductionNeeds();
        return true;
    }

    private void Kill()
    {
        _worldLayer.Kill(this);
        _onKill.ForEach(action => action.Invoke());
    }

    private void ReproductionNeeds()
    {
        Needs.Tick();
        var generalNeed = (_mind.GetWellBeing(Needs, _worldLayer.GetGlobalState()) + 1) * 50; // 0 to 100
        var reproductionRate = generalNeed * Random.Shared.NextDouble() + Random.Shared.Next(0, 20);

        if (_tickAge > 10 && reproductionRate > 100 - ReproductionRate)
        {
            Reproduce();
        }
    }

    private void Reproduce()
    {
        WorldLayer.Instance.EventLog.Log($"reproduced", this);
        Person child = _worldLayer.Container.Resolve<IAgentManager>().Spawn<Person, WorldLayer>().First();
        child.Position = Position.Copy();
        _worldLayer.InvokePersonReproduceHandler(this, child);
    }
}