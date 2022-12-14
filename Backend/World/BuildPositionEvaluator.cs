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
    private double[,] _restaurantScore;
    private double[,] _housingScoreBuffer;
    private double[,] _restaurantScoreBuffer;
    private PlannedStructure[,] _plannedBuildingType;
    public Safe2DArrayView<double> HousingScore => new(_housingScore);
    public Safe2DArrayView<double> RestaurantScore => new(_restaurantScore);

    private long _lastEvalutedTick = 0;

    private readonly PathFindingGrid _pathFindingGrid;
    private enum PlannedStructure
    {
        None = 0,
        Restaurant,
        Home
    }


    public BuildPositionEvaluator(Grid2D<Structure> structures)
    {
        _structures = structures;
        _housingScore = new double[structures.XSize, structures.YSize];
        _restaurantScore = new double[structures.XSize, structures.YSize];
        _housingScoreBuffer = new double[structures.XSize, structures.YSize];
        _restaurantScoreBuffer = new double[structures.XSize, structures.YSize];
        _plannedBuildingType = new PlannedStructure[structures.XSize, structures.YSize];

        float[,] tilesCosts = new float[WorldLayer.Instance.XSize, WorldLayer.Instance.YSize];
        for (int i = 0; i < WorldLayer.Instance.XSize; i++)
        for (int j = 0; j < WorldLayer.Instance.YSize; j++)
            tilesCosts[i, j] = 100;
        foreach (var structure in _structures)
        {
            float cost = structure is Street ? 0.1f : 0;
            tilesCosts[(int)structure.Position.X, (int)structure.Position.Y] = cost;
        }

        _pathFindingGrid = new PathFindingGrid(tilesCosts);
    }

    public void EvaluateBuildingScore()
    {
        lock (this)
        {
            _lastEvalutedTick = WorldLayer.CurrentTick;
            _housingScoreBuffer.Clear();
            _restaurantScoreBuffer.Clear();
            var structuresAt = new List<(int, int)>();
            for (int x = 0; x < _structures.XSize; x++)
            for (int y = 0; y < _structures.YSize; y++)
            {
                var structure = _structures[x, y];
                if (structure is not null || double.IsNegativeInfinity(_housingScore[x, y]) || 
                    double.IsNegativeInfinity(_restaurantScore[x, y]))
                {
                    structuresAt.Add((x, y));
                    continue;
                }

                var targetPosition = new double[] { x, y };

                var manhattanDistanceToNearestRestaurant =
                    Distance.Manhattan(NearestRestaurant(targetPosition).Position.PositionArray, targetPosition);

                const int width = 7;
                const int height = 7;
                IList<K2dTreeNode<Structure>>? buildingsNearby =
                    _structures.Kd.InsideRegion(new Hyperrectangle(x - width / 2, y - width / 2, width, height));
                var buildingsNearbyCount = buildingsNearby.Select(node => node.Value).OfType<House>().Count();
                var nearestPlannedRestaurantDistance = NearestPlannedPositionDistance(PlannedStructure.Restaurant, x, y);
                _restaurantScoreBuffer[x, y] = HousingNearbyScore(targetPosition) * Math.Min(Math.Min(manhattanDistanceToNearestRestaurant, 
                    nearestPlannedRestaurantDistance), 8);
                _housingScoreBuffer[x, y] = buildingsNearbyCount - manhattanDistanceToNearestRestaurant;
            }

            var min = _housingScoreBuffer.Min();
            _housingScoreBuffer = _housingScoreBuffer.Subtract(min);
            var max = _housingScoreBuffer.Max();
            _housingScoreBuffer = _housingScoreBuffer.Divide(max);
            
            min = _restaurantScoreBuffer.Min();
            _restaurantScoreBuffer = _restaurantScoreBuffer.Subtract(min);
            max = _restaurantScoreBuffer.Max();
            _restaurantScoreBuffer = _restaurantScoreBuffer.Divide(max);
            for (var i = 0; i < structuresAt.Count; i++)
            {
                _housingScoreBuffer[structuresAt[i].Item1, structuresAt[i].Item2] = double.NegativeInfinity;
                _restaurantScoreBuffer[structuresAt[i].Item1, structuresAt[i].Item2] = double.NegativeInfinity;
            }

            Buffer.BlockCopy(_housingScoreBuffer, 0, _housingScore, 0,
                sizeof(double) * _housingScore.GetLength(0) * _housingScore.GetLength(1));
            Buffer.BlockCopy(_restaurantScoreBuffer, 0, _restaurantScore, 0,
                sizeof(double) * _restaurantScore.GetLength(0) * _restaurantScore.GetLength(1));
        }
    }

    private Structure NearestRestaurant(double[] pos)
    {
        return _structures.Kd.Nearest(pos, 1, node => node.Value is Restaurant)
                   .FirstOrDefault().Node?.Value ??
               throw new InvalidOperationException("There should be at least one restaurant on the map");
    }

    private double HousingNearbyScore(double[] pos)
    {
        double score = 0;
        for (int x = 0; x < _structures.XSize; x++)
        for (int y = 0; y < _structures.YSize; y++)
        {
            var structure = _structures[x, y];
            if (structure is House)
            {
                score += 30 * Math.Pow(0.85, Distance.Manhattan(pos, new double[] { x, y }));
            }
        }

        return score;
    }

    private int NearestPlannedPositionDistance(PlannedStructure structure, int x, int y)
    {
        var radius = 1;
        var width = _plannedBuildingType.GetLength(0);
        var height = _plannedBuildingType.GetLength(1);

        while (radius < Math.Max(width, height))
        {
            // check top edge
            if (y - radius >= 0)
            {
                for (var currentX = x - radius; currentX <= x + radius; currentX++)
                {
                    if (currentX < 0 || currentX >= width)
                        continue;
                    if (_plannedBuildingType[currentX, y - radius] == structure)
                        return radius;
                }
            }
            // check bottom edge
            if (y + radius < height)
            {
                for (var currentX = x - radius; currentX <= x + radius; currentX++)
                {
                    if (currentX < 0 || currentX >= width)
                        continue;
                    if (_plannedBuildingType[currentX, y + radius] == structure)
                        return radius;
                }
            }
            // check left and right edges
            for (var currentY = y - radius + 1; currentY <= y + radius - 1; currentY++)
            {
                if (currentY < 0 || currentY >= height)
                    continue;
                if (x - radius >= 0 && _plannedBuildingType[x - radius, currentY] == structure)
                    return radius;
                if (x + radius < width && _plannedBuildingType[x + radius, currentY] == structure)
                    return radius;
            }
            radius++;
        }
        return int.MaxValue;
    }

    public Position GetNextHouseBuildPos()
    {
        lock (this)
        {
            if (_lastEvalutedTick != WorldLayer.CurrentTick)
                EvaluateBuildingScore();

            var (x, y) = _housingScore.ArgMax();
            _housingScore[x, y] = double.NegativeInfinity;
            _plannedBuildingType[x, y] = PlannedStructure.Home;
            BuildStreetToConnect(x, y);

            _pathFindingGrid.nodes[x, y].Update(false, x, y);
            return new Position(x, y);
        }
    }
    public Position GetNextRestaurantBuildPos()
    {
        lock (this)
        {
            if (_lastEvalutedTick != WorldLayer.CurrentTick)
                EvaluateBuildingScore();

            var (x, y) = _restaurantScore.ArgMax();
            _restaurantScore[x, y] = double.NegativeInfinity;
            _plannedBuildingType[x, y] = PlannedStructure.Restaurant;
            BuildStreetToConnect(x, y);

            _pathFindingGrid.nodes[x, y].Update(false, x, y);
            return new Position(x, y);
        }
    }

    private void BuildStreetToConnect(int x, int y)
    {
        if (!WorldLayer.Instance.Structures.GetAdjecent(x, y).OfType<Street>().Any())
        {
            var (restX, restY) = NearestRestaurant(new double[] { x, y }).Position.PositionArray;
            (restX, restY) = WorldLayer.Instance.Structures.GetAdjecent((int)restX, (int)restY).OfType<Street>().First()
                .Position.PositionArray;
            var streetRoute = _pathFindingGrid.FindPath(new PathFindingPoint(x, y),
                new PathFindingPoint((int)restX, (int)restY));

            BuildStreet(streetRoute);
        }
    }
    
    

    private void BuildStreet(PathFindingRoute streetRoute)
    {
        foreach (var (x, y) in streetRoute.RemainingPath)
        {
            if (WorldLayer.Instance.Structures[x, y] != null) break;
            WorldLayer.Instance.InsertStructure(new Street { Position = new Position(x, y) });
            _housingScore[x, y] = 0;
            _pathFindingGrid.nodes[x, y].price = 0.1f;
        }
    }

    public void ResetHousingScore(Position targetPosition)
    {
        lock (this)
        {
            _housingScore[(int)targetPosition.X, (int)targetPosition.Y] = 0;
            _plannedBuildingType[(int)targetPosition.X, (int)targetPosition.Y] = PlannedStructure.None;
        }
    }
    public void ResetRestaurantScore(Position targetPosition)
    {
        lock (this)
        {
            _housingScore[(int)targetPosition.X, (int)targetPosition.Y] = 0;
            _plannedBuildingType[(int)targetPosition.X, (int)targetPosition.Y] = PlannedStructure.None;
        }
    }
}