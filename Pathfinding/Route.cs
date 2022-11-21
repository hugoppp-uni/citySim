using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NesScripts.Controls.PathFind;

public class PathFindingRoute
{
    public static readonly PathFindingRoute CompletedRoute = new(new List<PathFindingPoint>());

    private readonly List<PathFindingPoint> _path;
    private int _alreadyVisited;
    public bool Completed => _alreadyVisited == _path.Count;

    public int Remaining => _path.Count - _alreadyVisited;

    internal PathFindingRoute(List<PathFindingPoint> path)
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