
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Agents.Behavior;
using CitySim.Backend.Entity.Structures;
using Mars.Numerics;

namespace CitySim.Backend.World;
public class Distances
{

    private const double DistanceScaler = 0.08;
    private readonly int _distanceToNearestRestaurant;
    private readonly double _distanceToHome;
    public static readonly int Count = 2;

    public Distances(Person person, WorldLayer layer)
    {
        _distanceToNearestRestaurant = layer.Structures.OfType<Restaurant>().Select((it) => 
            layer.FindRoute(person.Position,  it.Position).RemainingPath.Count()).Min();
        _distanceToHome = person.GetDistanceToAction(ActionType.Sleep);
    }

    /**
     * Normalize a value by using fast sigmoid
     */
    private static double Normalize(double x)
    {
        return Math.Pow(DistanceScaler * x +0.5, -1) - 1;
    }
    
    public double[] AsNormalizedArray()
    {
        return new [] { Normalize(_distanceToNearestRestaurant), Normalize(_distanceToHome)};
    }
}