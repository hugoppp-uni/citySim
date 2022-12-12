using CitySim.Backend.World;
using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

public interface IMind
{
    ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances);
    double GetWellBeing(PersonNeeds personNeeds, GlobalState globalState);
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

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState, Distances distances)
    {
        return Random.Shared.Next(5) == 0
            ? ActionType.BuildHouse
            : Enum.GetValues<ActionType>()[new[] { personNeeds.Sleepiness, personNeeds.Hunger }.ArgMin()];
    }

    public double GetWellBeing(PersonNeeds personNeeds, GlobalState globalState)
    {
        var global = globalState.AsNormalizedArray();
        var personal = personNeeds.AsNormalizedArray();
        return (personal.Sum() + global.Sum()) /
            (global.Length + personal.Length) * 2 - 1;
    }

    public void LearnFromDeath(ActionType neededActionToSurvive)
    {
        // ignore
    }
}