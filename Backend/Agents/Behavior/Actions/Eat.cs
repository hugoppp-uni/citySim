using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents.Behavior.Actions;

public class Eat : PersonAction
{
    public override string DescriptionNoun => "eat";
    public override string DescriptionVerb => "eating";

    public Eat(Person person, IGoapAgentStates agentStates, float cost = 0) : base(person, agentStates, cost)
    {
    }

    protected override bool ExecuteAction()
    {
        var oldHunger = Person.Hunger;
        Person.Hunger += 50;
        Person.Food--;

        Console.WriteLine($"{Person.Name} eat, Hunger: {oldHunger} -> {Person.Hunger}");
        return true;
    }

    public override Position TargetPosition => new(9, 9);
}