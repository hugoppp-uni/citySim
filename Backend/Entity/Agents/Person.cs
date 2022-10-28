﻿using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using CitySim.Backend.World;
using Mars.Common.Core.Collections;
using Mars.Interfaces.Agents;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NLog;
using ServiceStack.Script;

namespace CitySim.Backend.Entity.Agents;

public class Person : IAgent<WorldLayer>, IPositionableEntity
{
    public Guid ID { get; set; }
    public Position Position { get; set; } = null!; //Init()
    private WorldLayer _worldLayer = null!; //Init()
    private readonly Logger _logger = LogManager.GetCurrentClassLogger();

    public string Name { get; }
    private readonly IMind _mind;
    private readonly PersonRecollection _recollection = new();

    public PersonNeeds Needs { get; } = new();

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
        _mind = new PersonMind(0.5);
    }

    public void Init(WorldLayer layer)
    {
        _worldLayer = layer;
        Position = _worldLayer.RandomPosition();
        _worldLayer.GridEnvironment.Insert(this);

        var home = _worldLayer.Structures[^1];
        _recollection.Add(ActionType.Sleep, home.Position);
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
            $"{Name} (Hunger: {Needs}) {Position} can see: [{string.Join(',', personsInVicinity)}]");

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

        _logger.Trace($"{Name} is {_plannedAction}");
        if (_plannedAction.Execute() == ActionExecuter.Result.Executed)
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
            _logger.Trace($"{Name} DIED of starvation");
            return false;
        }

        return true;
    }

    private void Kill()
    {
        _worldLayer.Kill(this);
    }
}