using CitySim.Backend.Util;
using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

public interface IMind
{
    ActionType GetNextActionType(PersonNeeds personNeeds, double k, double i);
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
}

public class MindMock : IMind
{
    public static readonly IMind Instance = new MindMock();

    public ActionType GetNextActionType(PersonNeeds personNeeds, double k, double i)
    {
        return Enum.GetValues<ActionType>()[
            new[] { personNeeds.Sleepiness, personNeeds.Hunger}.ArgMin()];
    }
}