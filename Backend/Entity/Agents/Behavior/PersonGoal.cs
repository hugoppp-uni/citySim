using Mars.Components.Services.Planning;

namespace CitySim.Backend.Entity.Agents.Behavior;

public class PersonGoal : GoapGoal
{
    protected PersonGoal(IGoapAgentStates agentStates, float relevance = 0) : base(agentStates, relevance)
    {
    }

    public void UpdateRelevance(float relevance)
    {
        Relevance = relevance;
    }
}