using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Entity.Agents.Behavior.Actions;

public class Sleep : PersonAction
{
    public override string DescriptionNoun => "sleep";
    public override string DescriptionVerb => "sleeping";

    public Sleep(Person person, IGoapAgentStates agentStates, float cost = 0) : base(person, agentStates, cost)
    {
    }

    protected override bool ExecuteAction()
    {
        return true;
    }

    public override Position TargetPosition => new(1, 1);
}