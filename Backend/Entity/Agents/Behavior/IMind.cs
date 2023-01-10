using CitySim.Backend.World;
using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

public interface IMind
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="personNeeds"></param>
    /// <param name="globalState"></param>
    /// <param name="distances"></param>
    /// <param name="wellBeing">A normalized well being value calculated from the persons needs and the global state.
    /// The value can be between -1 and 1. A high value increased the chance of reproduction and can be used for debugging.</param>
    /// <returns></returns>
    ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances, double wellBeing);
    void LearnFromDeath(ActionType neededActionToSurvive);

    public static IMind Create(Type type)
    {
        if (!typeof(IMind).IsAssignableFrom(type))
            throw new InvalidOperationException($"{type} must implement IMind");

        return type.Name switch
        {
            nameof(PersonMind) => new PersonMind(0.5),
            _ => (IMind)Activator.CreateInstance(type)!
        };
    }
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

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances, double wellBeing)
    {
        return Random.Shared.Next(5) == 0
            ? ActionType.BuildHouse
            : Enum.GetValues<ActionType>()[new[] { personNeeds.Sleepiness, personNeeds.Hunger }.ArgMin()];
    }

    public void LearnFromDeath(ActionType neededActionToSurvive)
    {
        // ignore
    }
}