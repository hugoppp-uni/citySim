using Mars.Components.Services.Planning;

namespace CitySim.Backend.Agents.Behavior.Goals;

public class SleepingGoal : PersonGoal
{
    public SleepingGoal(IGoapAgentStates agentStates, float relevance = 0) : base(agentStates, relevance)
    {
    }
}