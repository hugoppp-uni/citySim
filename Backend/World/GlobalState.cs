namespace CitySim.Backend.World;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Numpy;

public class GlobalState
{
    public double Hunger { get; set; }
    public double Housing { get; set; }

    public GlobalState(int people, int units, int restaurants)
    {
        Housing = Math.Log10(1.0*units /people) / 2;// div by 2 to change the limit to 1
        Hunger = Math.Log10(5.0 * restaurants / people);// TODO: How to calculate
    }

    public double[] AsArray()
    {
        return new double[] { Hunger, Housing };
    }

    public NDarray<double> AsNdArray()
    {
        return new NDarray<double>(AsArray());
    }

    public double getGlobalWellBeing()
    {
        return AsArray().Sum();
    }
}
