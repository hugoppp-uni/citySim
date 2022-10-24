using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CitySim.Frontend
{
    internal class IsoMetricGrid
    {
        private readonly Matrix3x2 _transform;
        private readonly Matrix3x2 _transformInv;
        private readonly float _cellHeight;

        public IsoMetricGrid(float diagSpanX, float diagSpanY, float blockHeight)
        {
            if (diagSpanX <= 0)
                throw new ArgumentException("Value smaller or equal 0", nameof(diagSpanX));

            if (diagSpanY <= 0)
                throw new ArgumentException("Value smaller or equal 0", nameof(diagSpanY));

            _transform = new Matrix3x2(
                 diagSpanX / 2, -diagSpanY / 2,
                -diagSpanX / 2, -diagSpanY / 2,
                0, 0);

            Matrix3x2.Invert(_transform, out _transformInv);

            _cellHeight = blockHeight;
        }

        public Vector2 GetPosition2D(Vector3 position)
        {
            return Vector2.Transform(new(position.X, position.Y), _transform) + new Vector2(0, _cellHeight);
        }

        public IEnumerable<(int cell_x, int cell_y, Vector2 position2d, float cellHeight)> GetVisibleCells(Camera2D cam)
        {
            int screenWidth = GetScreenWidth();
            int screenHeight = GetScreenHeight();

            int maxX = (int)Vector2.Transform(GetScreenToWorld2D(new Vector2(screenWidth, 0), cam), _transformInv).X+2;
            int maxY = (int)Vector2.Transform(GetScreenToWorld2D(new Vector2(0, 0), cam), _transformInv).Y+2;
            int minX = (int)Vector2.Transform(GetScreenToWorld2D(new Vector2(0, screenHeight), cam), _transformInv).X-1;
            int minY = (int)Vector2.Transform(GetScreenToWorld2D(new Vector2(screenWidth, screenHeight), cam), _transformInv).Y-1;

            for (int x = maxX - 1; x >= minX; x--)
            {
                for (int y = maxY - 1; y >= minY; y--)
                {
                    yield return (x, y, GetPosition2D(new Vector3(x, y, 0)), _cellHeight);
                }
            }
        }
    }
}
