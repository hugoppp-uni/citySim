
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using Mars.Numerics;

namespace CitySim.Backend.World;
public class Distances
{

    private const double DistanceScaler = 0.08;
    private readonly int _distanceToClosedRestaurant;
    public static readonly int Count = 1;

    public Distances(Person person, WorldLayer layer)
    {
        _distanceToClosedRestaurant = layer.Structures.OfType<Restaurant>().Select((it) => 
            layer.FindRoute(person.Position,  it.Position).RemainingPath.Count()).Min();
    }

    /**
     * Normalize a value by using fast sigmoid
     */
    private static double Normalize(int x)
    {
        return Math.Pow(DistanceScaler * x +0.5, -1) - 1;
    }
    
    public double[] AsNormalizedArray()
    {
        return new [] { Normalize(_distanceToClosedRestaurant)};
    }
}