using NesScripts.Controls.PathFind;

namespace CitySim.Backend.Util;

public class PathFindingRoute
{
    public static readonly PathFindingRoute CompletedRoute = new(new List<PathFindingPoint>());

    private readonly List<PathFindingPoint> _path;
    private int _alreadyVisited;
    public bool Completed => _alreadyVisited == _path.Count;

    public PathFindingRoute(List<PathFindingPoint> path)
    {
        _path = path;
    }

    public PathFindingPoint Next()
    {
        if (_alreadyVisited > _path.Count)
            throw new InvalidOperationException();
        return _path[_alreadyVisited++];
    }

    public IEnumerable<PathFindingPoint> RemainingPath => _path.Skip(_alreadyVisited);
    public IEnumerable<PathFindingPoint> VisitedPath => _path.Take(_alreadyVisited);

    public double RemainingCost(PathFindingGrid pathFindingGrid)
    {
        return RemainingPath.Sum(point => pathFindingGrid.nodes[point.x, point.y].price);
    }
}