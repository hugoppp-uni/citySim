using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using Mars.Common;
using Mars.Common.Collections;
using Mars.Interfaces.Environments;
using Mars.Numerics;
using NesScripts.Controls.PathFind;

namespace CitySim.Backend.World;

public class BuildPositionEvaluator
{
    private readonly Grid2D<Structure> _structures;

    private double[,] _housingScore;
    public Safe2DArrayView<double> HousingScore => new(_housingScore);

    private long _lastEvalutedTick = 0;


    public BuildPositionEvaluator(Grid2D<Structure> structures)
    {
        _structures = structures;
        _housingScore = new double[structures.XSize, structures.YSize];
    }

    public void EvaluateHousingScore()
    {
        _lastEvalutedTick = WorldLayer.CurrentTick;

        for (int x = 0; x < _structures.XSize; x++)
        for (int y = 0; y < _structures.YSize; y++)
        {
            var structure = _structures[x, y];
            if (structure is not null)
                continue;

            var targetPosition = new double[] { x, y };

            var manhattanDistanceToRestaurant =
                Distance.Manhattan(NearestRestaurant(targetPosition).Position.PositionArray, targetPosition);

            const int width = 7;
            const int height = 7;
            IList<K2dTreeNode<Structure>>? buildingsNearby =
                _structures.Kd.InsideRegion(new Hyperrectangle(x - width / 2, y - width / 2, width, height));
            var buildingsNearbyCount = buildingsNearby.Select(node => node.Value).OfType<House>().Count();
            _housingScore[x, y] = buildingsNearbyCount - manhattanDistanceToRestaurant;
        }

        var min = _housingScore.Min();
        _housingScore = _housingScore.Subtract(min);
        var max = _housingScore.Max();
        _housingScore = _housingScore.Divide(max);
    }

    private Structure NearestRestaurant(double[] pos)
    {
        return _structures.OfType<Restaurant>().OrderBy(restaurant => Distance.Manhattan(restaurant.Position.PositionArray, pos)).First();
        //todo the nearest method does not respect the predicate. 
        return _structures.Kd.Nearest(pos, 1, node => node.Value.GetType() == typeof(Restaurant))
            .FirstOrDefault().Node?.Value ?? throw new InvalidOperationException("There should be at least one restaurant on the map");
    }

    public Position GetNextBuildPos()
    {
        lock (this)
        {
            if (_lastEvalutedTick != WorldLayer.CurrentTick)
                EvaluateHousingScore();

            var (x, y) = _housingScore.ArgMax();
            _housingScore[x, y] = 0;

            if (!WorldLayer.Instance.Structures.GetAdjecent(x, y).OfType<Street>().Any())
            {
                var (x2, y2) = NearestRestaurant(new double[] { x, y }).Position.PositionArray;
                //todo this class should have its own pathfinder, this doesn't work as expected
                var streetRoute = WorldLayer.Instance.FindRoute(x, y, (int)x2, (int)y2);
                BuildStreet(streetRoute);
            }


            return new Position(x, y);
        }
    }

    private void BuildStreet(PathFindingRoute streetRoute)
    {
        foreach (var (x, y) in streetRoute.RemainingPath)
        {
            if (WorldLayer.Instance.Structures[x, y] != null) break;
            WorldLayer.Instance.Structures.Add(new Street { Position = new Position(x, y) });
            _housingScore[x, y] = 0;
        }
    }
}