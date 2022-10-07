using Mars.Components.Services.Planning;
using Mars.Interfaces.Environments;

namespace CitySim.Backend.Agents.Behavior;

public abstract class PersonAction : GoapAction
{
    /**
     * The acting person
     */
    protected readonly Person Person;

    protected PersonAction(Person person, IGoapAgentStates agentStates, float cost = 0) : base(agentStates, cost)
    {
        Person = person;
    }

    public abstract Position GetTargetPosition();
}