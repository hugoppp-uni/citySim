﻿/**
 * Represent a point on the path-finding grid.
 * Based on code and tutorial by Sebastian Lague (https://www.youtube.com/channel/UCmtyQOKKmrMVaKuRXz02jbQ).
 *   
 * Author: Ronen Ness.
 * Since: 2016. 
*/

namespace NesScripts.Controls.PathFind
{
    /// <summary>
    /// A 2d point on the grid
    /// </summary>
    public struct PathFindingPoint
    {
        // point X
        public int x;

        // point Y
        public int y;

        /// <summary>
        /// Init the point with values.
        /// </summary>
        public PathFindingPoint(int iX, int iY)
        {
            this.x = iX;
            this.y = iY;
        }

        /// <summary>
        /// Init the point with a single value.
        /// </summary>
        public PathFindingPoint(PathFindingPoint b)
        {
            x = b.x;
            y = b.y;
        }

        /// <summary>
        /// Get point hash code.
        /// </summary>
        public override int GetHashCode()
        {
            return x ^ y;
        }

        /// <summary>
        /// Compare points.
        /// </summary>
        public override bool Equals(System.Object obj)
        {
            // check type
            if (!(obj.GetType() == typeof(PathFind.PathFindingPoint)))
                return false;

            // check if other is null
            PathFindingPoint p = (PathFindingPoint)obj;
            if (ReferenceEquals(null, p))
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        /// <summary>
        /// Compare points.
        /// </summary>
        public bool Equals(PathFindingPoint p)
        {
            // check if other is null
            if (ReferenceEquals(null, p))
            {
                return false;
            }

            // Return true if the fields match:
            return (x == p.x) && (y == p.y);
        }

        /// <summary>
        /// Check if points are equal in value.
        /// </summary>
        public static bool operator ==(PathFindingPoint a, PathFindingPoint b)
        {
            // If both are null, or both are same instance, return true.
            if (System.Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (ReferenceEquals(null, a))
            {
                return false;
            }

            if (ReferenceEquals(null, b))
            {
                return false;
            }

            // Return true if the fields match:
            return a.x == b.x && a.y == b.y;
        }

        /// <summary>
        /// Check if points are not equal in value.
        /// </summary>
        public static bool operator !=(PathFindingPoint a, PathFindingPoint b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Set point value.
        /// </summary>
        public PathFindingPoint Set(int iX, int iY)
        {
            this.x = iX;
            this.y = iY;
            return this;
        }

        public void Deconstruct(out int x, out int y)
        {
            x = this.x;
            y = this.y;
        }
    }
}