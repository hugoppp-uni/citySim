using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using CitySim.Backend.World;
using Mars.Core.Data;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Annotations;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NesScripts.Controls.PathFind;
using NLog;
using CircularBuffer;
using CitySim.Backend.Entity.Agents.Behavior.Actions;
using Mars.Common.Core.Collections;
using ServiceStack;

namespace CitySim.Backend.Entity.Agents;

public class Person : IAgent<WorldLayer>, IPositionableEntity
{
    public Guid ID { get; set; } = Guid.NewGuid();
    private const int ReproductionRate = 2;
    public Position Position { get; set; } = null!; //Init()
    private WorldLayer _worldLayer = null!; //Init()
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    private IMind _mind = null!;
    private readonly PersonRecollection _recollection = new();
    private readonly List<Action> _onKill = new();
    public bool IsAlive { get; private set; } = true;

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
        _mind = CitySim.Instance.GetMind();
        _worldLayer = layer;
        Position = _worldLayer.RandomBuildingPosition();
        _worldLayer.GridEnvironment.Insert(this);
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

        InitRecollection();

        if (_plannedAction is null)
        {
            _plannedAction = PlanNextAction();
            if (_plannedAction is null) return;

            _actionLog.PushFront(new PersonActionLog { Action = _plannedAction, Needs = Needs with { } });
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

    public double GetDistanceToAction(ActionType actionType)
    {
        return _recollection.ResolvePosition(actionType)
            .Select(it => Distance.Manhattan(it.PositionArray, Position.PositionArray))
            .Prepend(int.MaxValue)
            .MinBy(it => it);
    }


    private void InitRecollection()
    {
        if (!_recollection.ResolvePosition(ActionType.Sleep).Any())
        {
            lock (_worldLayer.Structures)
            {
                var home = _worldLayer.Structures.OfType<House>().OrderBy(it =>
                        _worldLayer.FindRoute(it.Position, Position).Remaining)
                    .FirstOrDefault(house => house.FreeSpaces > 0);
                if (home != null)
                {
                    _recollection.Add(ActionType.Sleep, home.Position);
                    _recollection.Add(ActionType.Work, home.Position);

                    home.AddInhabitant(this);
                    _onKill.Add(() => home.RemoveInhabitant(this));
                }
            }
        }

        if (!_recollection.ResolvePosition(ActionType.Eat).Any())
        {
            var restaurants = _worldLayer.Structures.OfType<Restaurant>().ToList();
            var randomRestaurant = restaurants[Random.Shared.Next(restaurants.Count)];
            _recollection.Add(ActionType.Eat, randomRestaurant.Position);
        }
    }

    private PersonAction? PlanNextAction()
    {
        var globalState = _worldLayer.GetGlobalState();
        var nextActionType = _mind.GetNextActionType(
            Needs,
            globalState,
            new Distances(this, _worldLayer),
            GetWellBeing(Needs, globalState)
        );

        Position? GetPosition() => nextActionType switch
        {
            ActionType.BuildHouse => _worldLayer.BuildPositionEvaluator.GetNextHouseBuildPos(),
            ActionType.BuildRestaurant => _worldLayer.BuildPositionEvaluator.GetNextRestaurantBuildPos(),
            ActionType.Eat => _worldLayer.Structures.OfType<Restaurant>().MinBy( it => 
                _worldLayer.FindRoute(Position,  it.Position).RemainingPath.Count())!.Position,
            _ => _recollection.ResolvePosition(nextActionType)
                .MinBy(position => Distance.Manhattan(position.PositionArray, Position.PositionArray))
        };


        var actionPosition = GetPosition();
        if (actionPosition != null)
        {
            return PersonAction.Create(nextActionType, actionPosition, this);
        }

        return null;
    }

    private bool ApplyGameRules()
    {
        Needs.Tick();
        if (Needs.Hunger < 0)
        {
            if (Needs.Money < EatAction.BurgerCost)
            {
                _mind.LearnFromDeath(ActionType.Work);
            }
            else
            {
                _mind.LearnFromDeath(ActionType.Eat);
            }

            Kill();
            WorldLayer.Instance.EventLog.Log($"DIED of starvation with {Needs.Money} money", this);
            return false;
        }

        if (Needs.Sleepiness < 0)
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
        _plannedAction?.CleanUp();
        IsAlive = false;
    }

    private void ReproductionNeeds()
    {
        Needs.Tick();
        var generalNeed = (GetWellBeing(Needs, _worldLayer.GetGlobalState()) + 1) * 50; // 0 to 100
        var reproductionRate = generalNeed * Random.Shared.NextDouble() + Random.Shared.Next(0, 20);

        if (_tickAge > 10 && reproductionRate > 100 - ReproductionRate)
        {
            Reproduce();
        }
    }
    
    private double GetWellBeing(PersonNeeds personNeeds, GlobalState globalState)
    {
        var global = globalState.AsNormalizedArray();
        var personal = personNeeds.AsNormalizedArray();
        return (personal.Sum() + global.Sum()) /
            (global.Length + personal.Length);
    }

    private void Reproduce()
    {
        WorldLayer.Instance.EventLog.Log($"reproduced", this);
        Person child = _worldLayer.Container.Resolve<IAgentManager>().Spawn<Person, WorldLayer>().First();
        child.Position = Position.Copy();
        child.Needs.Money = Needs.Money / 2;
        Needs.Money -= child.Needs.Money;
        _worldLayer.InvokePersonReproduceHandler(this, child);
    }
}