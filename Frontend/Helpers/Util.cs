using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CitySim.Frontend.Helpers
{
    internal class Util
    {
        public static float EaseOutElastic(float x)
        {
            float c4 = 2 * MathF.PI / 3;

            return x == 0
              ? 0
              : x == 1
              ? 1
              : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * c4) + 1;
        }


        //from https://stackoverflow.com/a/9755252 with slight modifications

        /// <summary>
        /// Checks if a given point is inside a Triangle made of the given points
        /// </summary>
        /// <param name="p">The point to check</param>
        /// <param name="a">Point A of the triangle</param>
        /// <param name="b">Point B of the triangle</param>
        /// <param name="c">Point C of the triangle</param>
        /// <returns> <see langword="true"/> if the point lies inside the triangle, <see langword="false"/> otherwise</returns>
        public static bool IsPointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float AP_x = p.X - a.X;
            float AP_y = p.Y - a.Y;

            float CP_x = p.X - b.X;
            float CP_y = p.Y - b.Y;

            bool s_ab = (b.X - a.X) * AP_y - (b.Y - a.Y) * AP_x > 0.0;

            if (/*s_ac*/   (c.X - a.X) * AP_y - (c.Y - a.Y) * AP_x > 0.0 == s_ab) return false;

            if (/*s_cb*/   (c.X - b.X) * CP_y - (c.Y - b.Y) * CP_x > 0.0 != s_ab) return false;

            return true;
        }


        /// <summary>
        /// Checks if a given point is inside a Quad made of the given points
        /// </summary>
        /// <param name="p">The point to check</param>
        /// <param name="a">Point A of the quad</param>
        /// <param name="b">Point B of the quad</param>
        /// <param name="c">Point C of the quad</param>
        /// <param name="d">Point D of the quad</param>
        /// <returns> <see langword="true"/> if the point lies inside the quad, <see langword="false"/> otherwise</returns>
        public static bool IsPointInQuad(Vector2 p, Vector2 a, Vector2 b, Vector2 c, Vector2 d)
        {
            float AP_x = p.X - a.X;
            float AP_y = p.Y - a.Y;

            float CP_x = p.X - c.X;
            float CP_y = p.Y - c.Y;

            bool s_ab = (b.X - a.X) * AP_y - (b.Y - a.Y) * AP_x > 0.0;

            if (/*s_ad*/   (d.X - a.X) * AP_y - (d.Y - a.Y) * AP_x > 0.0 == s_ab) return false;

            if (/*s_cb*/   (b.X - c.X) * CP_y - (b.Y - c.Y) * CP_x > 0.0 == s_ab) return false;

            if (/*s_cd*/   (d.X - c.X) * CP_y - (d.Y - c.Y) * CP_x > 0.0 != s_ab) return false;

            return true;
        }
    }
}
