using System.ComponentModel.DataAnnotations;

namespace CitySim.Backend.World;
using System;

public class GlobalState
{
    public double RestaurantScoreAverage { get; }
    public double Housing { get;  }

    public GlobalState(int people, int units, double restaurantScoreAverage)
    {
        Housing = ((double)units)/people;
        RestaurantScoreAverage = restaurantScoreAverage; 
    }

    /**
     * Normalize an open ranged positive integer where the best value is at x = 1
     */
    private static double NormalizeFraction_optimumAt1(double x)
    {
        return 1.66 * x * Math.Pow(Math.E, -0.5 * x * x );
    }

    /// <summary>
    /// Returns the values of the personal needs as array.
    /// The Values gets normalized to values between -1 and 1 
    /// </summary>
    public double[] AsNormalizedArray()
    {
        return new [] { RestaurantScoreAverage, NormalizeFraction_optimumAt1(Housing) };
    }
}
