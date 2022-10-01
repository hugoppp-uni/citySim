using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents.Behavior.Actions;

public class Sleep : PersonAction
{
    public Sleep(Person person, IGoapAgentStates agentStates, float cost = 0) : base(person, agentStates, cost)
    {
    }

    protected override bool ExecuteAction()
    {
        return true;
    }

    public override Position GetTargetPosition() => new(1, 1);
}