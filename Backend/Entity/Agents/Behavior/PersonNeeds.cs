using Mars.Numerics;

namespace CitySim.Backend.Entity.Agents.Behavior;

/// <summary>
/// Value = 0 is bad
/// Value = 1 is good
/// </summary>
public record PersonNeeds
{
    public double Sleepiness { get; internal set; } = 0.4 + Random.Shared.NextDouble() * 0.6;
    public double Hunger { get; internal set; } = 0.4 + Random.Shared.NextDouble() * 0.6;
    public int Money { get; internal set; } =  5 + Random.Shared.Next(6);

    internal void Tick()
    {
        Hunger -= 0.01;
        Sleepiness -= 0.015;
    }

    /// <summary>
    /// Returns the values of the personal needs as array.
    /// The Values gets normalized to values between -1 and 1 
    /// </summary>
    internal double[] AsNormalizedArray()
    {
        return new []{Normalize(Sleepiness), Normalize(Hunger), NormalizeMoney(Money)};
    }
    
    private static double Normalize(double x)
    {
        return x * 2 - 1;
    }
    
    private static double NormalizeMoney(double x)
    {
        return Math.Log(x + 1, 4) - 1;
    }

    public override string ToString()
    {
        return $"Hunger: {Hunger:F2}\tSleepiness: {Sleepiness:F2}\tMoney: {Money:F2}";
    }
}