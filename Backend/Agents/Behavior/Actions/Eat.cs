using Mars.Components.Services.Planning;

namespace CitySim.Backend.Agents.Behavior.Actions;

public class Eat : GoapAction
{
    private readonly Person _person;

    public Eat(Person person, IGoapAgentStates agentStates, float cost = 0) : base(agentStates, cost)
    {
        _person = person;
    }

    protected override bool ExecuteAction()
    {

        var oldHunger = _person.Hunger;
        _person.Hunger += 2;
        _person.Food--;
        
        Console.WriteLine($"{_person.Name} eat, Hunger: {oldHunger} -> {_person.Hunger}");
        return true;
    }
}