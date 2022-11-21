using System.ComponentModel.DataAnnotations;

namespace CitySim.Backend.World;
using System;

public class GlobalState
{
    public int Hunger { get; }
    public int Housing { get;  }

    public GlobalState(int people, int units, int restaurantCapacity)
    {
        Housing = units/people;
        Hunger = restaurantCapacity/people; 
    }

    /**
     * Normalize a value by using fast sigmoid
     */
    private static double Normalize(int x)
    {
        return 1.66 * x * Math.Pow(Math.E, 0.5 * x * x );
    }

    /// <summary>
    /// Returns the values of the personal needs as array.
    /// The Values gets normalized to values between -1 and 1 
    /// </summary>
    public double[] AsNormalizedArray()
    {
        return new [] { Normalize(Hunger), Normalize(Housing) };
    }
}
