using Mars.Components.Services.Planning;

namespace CitySim.Backend.Agents.Behavior;

public class PersonGoal : GoapGoal
{
    public PersonGoal(IGoapAgentStates agentStates, float relevance = 0) : base(agentStates, relevance)
    {
    }

    public void UpdateRelevance(float relevance)
    {
        Relevance = relevance;
    }
}