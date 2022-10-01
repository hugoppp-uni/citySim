using CitySim.Backend.Agents.Behavior.Actions;
using CitySim.Backend.Agents.Behavior.Goals;
using Mars.Components.Services.Planning;

namespace CitySim.Backend.Agents.Behavior;

public class PersonGoap
{
    private readonly Person _person;
    private readonly GoapAgentStates _states = new();
    private readonly GoapStateKey<bool> _keyHunger = new("hunger");
    private readonly GoapStateKey<bool> _keyHasFood = new("food");
    private GoapPlanner _goapPlanner;

    public PersonGoap(Person person)
    {
        _person = person;
        _goapPlanner = new(_states);
        InitPlanner();
    }

    public void Tick()
    {
        ResetProperties();
    }

    public IList<IGoapAction> Plan()
    {
        return _goapPlanner.Plan();
    }

    // see reference:
    // https://git.haw-hamburg.de/mars/mars-learning/-/blob/main/Examples/Summer2022%20-%20Student%20Models/q-learning-goap-stronghold/Stronghold/Model/Agent/Villager.cs#L83
    private void InitPlanner()
    {
        // adding states
        ResetProperties();

        // creating actions
        var actionEat = new Eat(_person, _states);
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

    private void ResetProperties()
    {
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHunger, _person.Hunger < 8));
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHasFood, _person.Food > 0));
    }
}