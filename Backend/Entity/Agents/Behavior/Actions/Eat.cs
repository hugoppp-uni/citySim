using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;
using NLog;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

public class Eat : PersonAction
{
    public override string DescriptionNoun => "eat";
    public override string DescriptionVerb => "eating";

    public Eat(Person person, IGoapAgentStates agentStates, float cost = 0) : base(person, agentStates, cost)
    {
    }

    protected override bool ExecuteAction()
    {
        Person.Hunger += 50;
        Person.Food--;
        return true;
    }

    public override Position TargetPosition => new(9, 9);
}