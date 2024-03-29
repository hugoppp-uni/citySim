﻿/**
 * Provide simple path-finding algorithm with tile prices support.
 * Based on code and tutorial by Sebastian Lague (https://www.youtube.com/channel/UCmtyQOKKmrMVaKuRXz02jbQ).
 *   
 * Author: Ronen Ness.
 * Since: 2016. 
*/
using System.Collections.Generic;
using System.Linq;

namespace NesScripts.Controls.PathFind
{
    /// <summary>
    /// Main class to find the best path to walk from A to B.
    /// 
    /// Usage example:
    /// Grid grid = new Grid(width, height, tiles_costs);
    /// List<Point> path = Pathfinding.FindPath(grid, from, to);
    /// </summary>
    public class Pathfinding
    {
        /// <summary>
        /// Different ways to calculate path distance.
        /// </summary>
		public enum DistanceType
		{
            /// <summary>
            /// The "ordinary" straight-line distance between two points.
            /// </summary>
			Euclidean,

            /// <summary>
            /// Distance without diagonals, only horizontal and/or vertical path lines.
            /// </summary>
			Manhattan
        }

        /// <summary>
        /// Find a path between two points.
        /// </summary>
        /// <param name="pathFindingGrid">Grid to search.</param>
        /// <param name="startPos">Starting position.</param>
		/// <param name="targetPos">Ending position.</param>
        /// <param name="distance">The type of distance, Euclidean or Manhattan.</param>
        /// <param name="ignorePrices">If true, will ignore tile price (how much it "cost" to walk on).</param>
        /// <returns>List of points that represent the path to walk.</returns>
		public static PathFindingRoute FindPath(PathFindingGrid pathFindingGrid, PathFindingPoint startPos, PathFindingPoint targetPos, DistanceType distance = DistanceType.Euclidean, bool ignorePrices = false)
        {
            // find path
            List<PathFindingNode> nodes_path = _ImpFindPath(pathFindingGrid, startPos, targetPos, distance, ignorePrices);

            return nodes_path == null
                ? PathFindingRoute.CompletedRoute
                : new PathFindingRoute(nodes_path.Select(node => new PathFindingPoint(node.gridX, node.gridY)).ToList());
        }

        /// <summary>
        /// Internal function that implements the path-finding algorithm.
        /// </summary>
        /// <param name="pathFindingGrid">Grid to search.</param>
        /// <param name="startPos">Starting position.</param>
        /// <param name="targetPos">Ending position.</param>
        /// <param name="distance">The type of distance, Euclidean or Manhattan.</param>
        /// <param name="ignorePrices">If true, will ignore tile price (how much it "cost" to walk on).</param>
        /// <returns>List of grid nodes that represent the path to walk.</returns>
        private static List<PathFindingNode> _ImpFindPath(PathFindingGrid pathFindingGrid, PathFindingPoint startPos, PathFindingPoint targetPos, DistanceType distance = DistanceType.Euclidean, bool ignorePrices = false)
        {
            PathFindingNode startNode = pathFindingGrid.nodes[startPos.x, startPos.y];
            PathFindingNode targetNode = pathFindingGrid.nodes[targetPos.x, targetPos.y];

            List<PathFindingNode> openSet = new List<PathFindingNode>();
            HashSet<PathFindingNode> closedSet = new HashSet<PathFindingNode>();
            openSet.Add(startNode);

            while (openSet.Count > 0)
            {
                PathFindingNode currentNode = openSet[0];
                for (int i = 1; i < openSet.Count; i++)
                {
                    if (openSet[i].fCost < currentNode.fCost || openSet[i].fCost == currentNode.fCost && openSet[i].hCost < currentNode.hCost)
                    {
                        currentNode = openSet[i];
                    }
                }

                openSet.Remove(currentNode);
                closedSet.Add(currentNode);

                if (currentNode == targetNode)
                {
                    return RetracePath(pathFindingGrid, startNode, targetNode);
                }

                foreach (PathFindingNode neighbour in pathFindingGrid.GetNeighbours(currentNode, distance))
                {
                    if (!neighbour.walkable || closedSet.Contains(neighbour))
                    {
                        continue;
                    }

                    int newMovementCostToNeighbour = currentNode.gCost + GetDistance(currentNode, neighbour) * (ignorePrices ? 1 : (int)(10.0f * neighbour.price));
                    if (newMovementCostToNeighbour < neighbour.gCost || !openSet.Contains(neighbour))
                    {
                        neighbour.gCost = newMovementCostToNeighbour;
                        neighbour.hCost = GetDistance(neighbour, targetNode);
                        neighbour.parent = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Retrace path between two points.
        /// </summary>
        /// <param name="pathFindingGrid">Grid to work on.</param>
        /// <param name="startNode">Starting node.</param>
        /// <param name="endNode">Ending (target) node.</param>
        /// <returns>Retraced path between nodes.</returns>
        private static List<PathFindingNode> RetracePath(PathFindingGrid pathFindingGrid, PathFindingNode startNode, PathFindingNode endNode)
        {
            List<PathFindingNode> path = new List<PathFindingNode>();
            PathFindingNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.parent;
            }
            path.Reverse();
            return path;
        }

        /// <summary>
        /// Get distance between two nodes.
        /// </summary>
        /// <param name="nodeA">First node.</param>
        /// <param name="nodeB">Second node.</param>
        /// <returns>Distance between nodes.</returns>
        private static int GetDistance(PathFindingNode nodeA, PathFindingNode nodeB)
        {
            int dstX = System.Math.Abs(nodeA.gridX - nodeB.gridX);
            int dstY = System.Math.Abs(nodeA.gridY - nodeB.gridY);
            return (dstX > dstY) ? 
                14 * dstY + 10 * (dstX - dstY) :
                14 * dstX + 10 * (dstY - dstX);
        }
    }

}