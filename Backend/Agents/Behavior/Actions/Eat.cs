using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents.Behavior.Actions;

public class Eat : PersonAction
{
    public Eat(Person person, IGoapAgentStates agentStates, float cost = 0) : base(person, agentStates, cost)
    {
    }

    protected override bool ExecuteAction()
    {
        var oldHunger = Person.Hunger;
        Person.Hunger += 15;
        Person.Food--;

        Console.WriteLine($"{Person.Name} eat, Hunger: {oldHunger} -> {Person.Hunger}");
        return true;
    }

    public override Position GetTargetPosition() => new(9, 9);
}