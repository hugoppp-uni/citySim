using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

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
        //Money -= 0.01;
    }

    /// <summary>
    /// Returns the values of the personal needs as array.
    /// The Values gets normalized to values between -1 and 1 
    /// </summary>
    public double[] AsNormalizedArray()
    {
        return new []{Normalize(Sleepiness), Normalize(Hunger), Normalize(Money)};
    }
    
    private static double Normalize(double x)
    {
        return x * 2 - 1;
    }

    public double GetWellBeing()
    {
        return AsNormalizedArray().Sum();
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
        return Enum.GetValues<ActionType>()[
            new[] { personNeeds.Sleepiness, personNeeds.Hunger}.ArgMin()];
    }
}