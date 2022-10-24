namespace CitySim.Backend.World;
using System;
using System.Linq;

public class GlobalState
{
    public double Hunger { get; set; }
    public double Housing { get; set; }

    public GlobalState(int people, int units, int restaurantCapacity)
    {
        // Suggestion
        // Map value to values between -1 and 1
        Housing = Normalize(units - people);
        Housing = Normalize(restaurantCapacity - people); 
    }

    /**
     * Normalize a value by using fast sigmoid
     */
    private static double Normalize(int x)
    {
        return x * 1.0 / (1 + Math.Abs(x));
    }

    public double[] AsArray()
    {
        return new [] { Hunger, Housing };
    }

    public double GetGlobalWellBeing()
    {
        return AsArray().Sum();
    }
}
