using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents.Behavior;

public abstract class PersonAction : GoapAction
{
    public PersonAction(Person person, IGoapAgentStates agentStates, float cost = 0) : base(agentStates, cost)
    {
    }

    public abstract Position GetTargetPosition();
}