using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Raylib_CsLo.Raylib;
using Raylib_CsLo;

namespace CitySim.Frontend
{
    internal class RaylibExtensions
    {
        public static void MyDrawRect(float left, float top, float right, float bottom, Color color)
        {
            DrawRectangleRec(new(left, top, right - left, bottom - top), color);
        }

        public static void MyDrawRoundedRect(float left, float top, float right, float bottom, float radius, Color color)
        {
            float width = right - left;
            float height = bottom - top;

            //raylib decided to handle roundness in a pretty annoying way
            float roundness = radius / (Math.Min(width, height) / 2);

            DrawRectangleRounded(new(left, top, width, height), roundness, 6, color);
        }
    }
}
