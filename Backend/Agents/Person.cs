using CitySim.Backend.Agents.Behavior.Actions;
using CitySim.Backend.Agents.Behavior.Goals;
using CitySim.Backend.World;
using Mars.Common;
using Mars.Components.Services.Planning;
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

    private readonly GoapAgentStates _states = new();
    private readonly GoapStateKey<bool> _keyHunger = new("hunger");
    private readonly GoapStateKey<bool> _keyHasFood = new("food");
    private GoapPlanner _goapPlanner;


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
    }

    public void Init(GridLayer layer)
    {
        _gridLayer = layer;
        Position = _gridLayer.RandomPosition();
        _gridLayer.GridEnvironment.Insert(this);
        InitPlanner();
        Console.WriteLine($"Agent {ID} init");
    }

    // see reference:
    // https://git.haw-hamburg.de/mars/mars-learning/-/blob/main/Examples/Summer2022%20-%20Student%20Models/q-learning-goap-stronghold/Stronghold/Model/Agent/Villager.cs#L83
    private void InitPlanner()
    {
        // adding states
        ResetProperties();

        // creating actions
        var actionEat = new Eat(this, _states);
        actionEat.AddOrUpdateEffect(_keyHunger, false);
        actionEat.AddOrUpdatePrecondition(_keyHasFood, true);

        // creating goals
        var goalHungry = new HungerGoal(_states);
        goalHungry.AddOrUpdateDesiredState(_keyHunger, false);
        goalHungry.AddAction(actionEat);

        // setting relevance

        // adding goals to planner
        _goapPlanner = new GoapPlanner(_states);
        _goapPlanner.AddGoal(goalHungry);

    }

    public void Tick()
    {
        var personsInVicinity = _gridLayer.GridEnvironment.Explore(Position, radius: 2)
            .Where(p => p.ID != ID)
            .Select(p => p.Name);
        Console.WriteLine(
            $"{Name} (Hunger: {Hunger}, Food: {Food}) {Position} can see: [{string.Join(',', personsInVicinity)}]");

        var centerBearing = Position.GetBearing(_gridLayer.GridEnvironment.Centre);
        _gridLayer.GridEnvironment.MoveTowards(this, centerBearing, distanceToPass: 1);

        Hunger--;
        ResetProperties();

        if (Hunger < 0)
        {
            _gridLayer.GridEnvironment.Remove(this);
            _gridLayer.UnregisterAgent(_gridLayer, this);
            Console.WriteLine($"{Name} DIED of starvation");
        }

        var goapActions = _goapPlanner.Plan().ToList();
        if (goapActions.Any(action => action is not AllGoalsSatisfiedAction))
        {
            goapActions.First().Execute();
        }
    }

    private void ResetProperties()
    {
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHunger, Hunger < 8));
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHasFood, Food > 0));
    }
}