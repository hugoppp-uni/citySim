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
    private double[,] _housingScoreBuffer;
    float[,] tilesCosts = new float[WorldLayer.XSize, WorldLayer.YSize];
    public Safe2DArrayView<double> HousingScore => new(_housingScore);

    private long _lastEvalutedTick = 0;

    private readonly PathFindingGrid _pathFindingGrid;


    public BuildPositionEvaluator(Grid2D<Structure> structures)
    {
        _structures = structures;
        _housingScore = new double[structures.XSize, structures.YSize];
        _housingScoreBuffer = new double[structures.XSize, structures.YSize];

        for (int i = 0; i < WorldLayer.XSize; i++)
        for (int j = 0; j < WorldLayer.YSize; j++)
            tilesCosts[i, j] = 100;
        foreach (var structure in _structures)
        {
            float cost = structure is Street ? 0.1f : 0;
            tilesCosts[(int)structure.Position.X, (int)structure.Position.Y] = cost;
        }

        _pathFindingGrid = new PathFindingGrid(tilesCosts);
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
            _housingScoreBuffer[x, y] = buildingsNearbyCount - manhattanDistanceToRestaurant;
        }

        var min = _housingScoreBuffer.Min();
        _housingScoreBuffer = _housingScoreBuffer.Subtract(min);
        var max = _housingScoreBuffer.Max();
        _housingScoreBuffer = _housingScoreBuffer.Divide(max);

        Buffer.BlockCopy(_housingScoreBuffer, 0, _housingScore, 0,
            sizeof(double) * _housingScore.GetLength(0) * _housingScore.GetLength(1));
    }

    private Structure NearestRestaurant(double[] pos)
    {
        return _structures.OfType<Restaurant>()
            .OrderBy(restaurant => Distance.Manhattan(restaurant.Position.PositionArray, pos)).First();
        //todo the nearest method does not respect the predicate. 
        return _structures.Kd.Nearest(pos, 1, node => node.Value.GetType() == typeof(Restaurant))
                   .FirstOrDefault().Node?.Value ??
               throw new InvalidOperationException("There should be at least one restaurant on the map");
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
                (x2, y2) = WorldLayer.Instance.Structures.GetAdjecent((int)x2, (int)y2).OfType<Street>().First()
                    .Position.PositionArray;
                //todo this class should have its own pathfinder, this doesn't work as expected
                PathFindingRoute streetRoute;
                streetRoute = _pathFindingGrid.FindPath(new PathFindingPoint(x, y),
                    new PathFindingPoint((int)x2, (int)y2));
                Thread.Sleep(10);

                if (!streetRoute.Completed)
                    BuildStreet(streetRoute);
                _pathFindingGrid.UpdateGrid(tilesCosts);
            }

            tilesCosts[x, y] = 0;
            return new Position(x, y);
        }
    }

    private void BuildStreet(PathFindingRoute streetRoute)
    {
        foreach (var (x, y) in streetRoute.RemainingPath)
        {
            if (WorldLayer.Instance.Structures[x, y] != null) break;
            WorldLayer.Instance.InsertStructure(new Street { Position = new Position(x, y) });
            _housingScore[x, y] = 0;
            tilesCosts[x, y] = 0;
        }
    }

    public static void Print2DArray<T>(T[,] matrix, PathFindingRoute route)
    {
        var pathFindingPoints = route.RemainingPath.ToHashSet();
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                if (pathFindingPoints.Contains(new PathFindingPoint(i, j)))
                    Console.BackgroundColor = ConsoleColor.DarkRed;
                else
                {
                    Console.ResetColor();
                }

                Console.Write(matrix[i, j] + "\t");
            }

            Console.WriteLine();
        }
    }
}