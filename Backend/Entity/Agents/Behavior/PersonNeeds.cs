using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

/// <summary>
/// Value = 0 is bad
/// Value = 1 is good
/// </summary>
public record PersonNeeds
{
    public double Sleepiness { get; set; } = 0.4 + Random.Shared.NextDouble() * 0.6;
    public double Hunger { get; set; } = 0.4 + Random.Shared.NextDouble() * 0.6;
    public double Money { get; set; } = 1;//0.4 + Random.Shared.NextDouble() * 0.6;

    public void Tick()
    {
        Hunger -= 0.02;
        Sleepiness -= 0.02;
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
}