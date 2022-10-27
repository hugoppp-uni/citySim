using CitySim.Backend.Util;
using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

using Numpy;
using World;

public interface IMind
{
    ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState);
}

/// <summary>
/// Value = 0 is bad
/// Value = 1 is good
/// </summary>
public record PersonNeeds
{
    public double Sleepiness { get; set; }
    public double Hunger { get; set; }
    public double Money { get; set; }

    public PersonNeeds()
    {
        Hunger = Sleepiness = Money = 1;
    }

    public void Tick()
    {
        Hunger -= 0.01;
        Sleepiness -= 0.01;
        Money -= 0.01;
    }

    public double[] AsArray()
    {
        return new double[] { Sleepiness, Hunger, Money };
    }

    public NDarray<double> AsNdArray()
    {
        return new NDarray<double>(AsArray());
    }

    public double getWellBeing()
    {
        return AsArray().Sum();
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

    public ActionType GetNextActionType(PersonNeeds personNeeds, GlobalState globalState)
    {
        return Random.Shared.Next(5) == 0
            ? ActionType.BuildHouse
            : Enum.GetValues<ActionType>()[new[] { personNeeds.Sleepiness, personNeeds.Hunger }.ArgMin()];
    }
}