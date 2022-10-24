using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Util;
using Mars.Common;
using Mars.Common.Collections;
using Mars.Numerics;

namespace CitySim.Backend.World;

public class BuildPositionEvaluator
{
    private readonly StructureCollection _structureCollection;
    
    private double[,] _housingScore;
    public Safe2DArrayView<double> HousingScore => new (_housingScore);
    

    public BuildPositionEvaluator(StructureCollection structureCollection)
    {
        _structureCollection = structureCollection;
        _housingScore = new double[structureCollection.XSize, structureCollection.YSize];
    }

    public void EvaluateHousingScore()
    {
        for (int x = 0; x < _structureCollection.XSize; x++)
        for (int y = 0; y < _structureCollection.YSize; y++)
        {
            var structure = _structureCollection[x, y];
            if (structure is not null)
                continue;

            var targetPosition = new double[] { x, y };
            Structure? nearestRestaurant =
                _structureCollection.Kd.Nearest(targetPosition, 1, node => node.Value.GetType() == typeof(Restaurant))
                    .FirstOrDefault().Node?.Value;

            var manhattanDistanceToRestaurant =
                nearestRestaurant is null
                    ? _structureCollection.XSize + _structureCollection.YSize + 1
                    : Distance.Manhattan(nearestRestaurant.Position.PositionArray, targetPosition);

            const int width = 7;
            const int height = 7;
            IList<K2dTreeNode<Structure>>? buildingsNearby =
                _structureCollection.Kd.InsideRegion(new Hyperrectangle(x - width / 2, y - width / 2, width, height));
            var buildingsNearbyCount = buildingsNearby.Select(node => node.Value).OfType<House>().Count();
            _housingScore[x, y] = buildingsNearbyCount - manhattanDistanceToRestaurant;
        }

        var argMax = _housingScore.ArgMax();

        var min = _housingScore.Min();
        _housingScore = _housingScore.Subtract(min);
        var max = _housingScore.Max();
        _housingScore = _housingScore.Divide(max);
    }
}