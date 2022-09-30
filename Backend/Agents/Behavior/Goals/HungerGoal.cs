using Mars.Components.Services.Planning;

namespace CitySim.Backend.Agents.Behavior.Goals;

public class HungerGoal : GoapGoal
{
    public HungerGoal(IGoapAgentStates agentStates, float relevance = 0) : base(agentStates, relevance)
    {
    }
}