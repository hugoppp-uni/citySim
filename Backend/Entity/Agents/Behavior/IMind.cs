using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

using World;

public interface IMind
{
    ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState);
}

public class MindMock : IMind
{
    public static readonly IMind Instance = new MindMock(1, 1);
    private double c, i;

    public MindMock(double collectiveFactor, double individualFactor)
    {
        c = collectiveFactor;
        i = individualFactor;
    }

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        return Random.Shared.Next(5) == 0
            ? ActionType.BuildHouse
            : Enum.GetValues<ActionType>()[new[] { personNeeds.Sleepiness, personNeeds.Hunger }.ArgMin()];
    }
}