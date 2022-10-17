using Mars.Components.Services.Planning;

namespace CitySim.Backend.Entity.Agents.Behavior.Goals;

public class HungerGoal : PersonGoal
{
    public HungerGoal(IGoapAgentStates agentStates, float relevance = 0) : base(agentStates, relevance)
    {
    }
}