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
    private readonly GoapStateKey<bool> _keySleepy = new("sleepy");
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

        var actionSleep = new Sleep(_person, _states);
        actionSleep.AddOrUpdateEffect(_keySleepy, false);

        // creating goals
        var goalHungry = new HungerGoal(_states);
        goalHungry.AddOrUpdateDesiredState(_keyHunger, false);
        goalHungry.AddAction(actionEat);

        var goalSleep = new SleepingGoal(_states);
        goalSleep.AddAction(actionSleep);
        goalSleep.AddOrUpdateDesiredState(_keySleepy, false);
        
        // setting relevance
        
        goalHungry.UpdateRelevance(0.9f);
        goalSleep.UpdateRelevance(0.1f);

        // adding goals to planner
        _goapPlanner = new GoapPlanner(_states);
        _goapPlanner.AddGoal(goalHungry);
        _goapPlanner.AddGoal(goalSleep);
    }

    private void ResetProperties()
    {
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHunger, _person.Hunger < 10));
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keyHasFood, _person.Food > 0));
        _states.AddOrUpdateState(new GoapStateProperty<bool>(_keySleepy, true));
    }
}