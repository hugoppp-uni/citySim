using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;

namespace CitySim.Frontend
{
    internal class CitySimView
    {
        private WorldDrawer _worldDrawer;
        private Camera2D _cam;

        public CitySimView(Backend.CitySim citySim)
        {
            _worldDrawer = new(citySim);

            _cam = new()
            {
                target = _worldDrawer.Grid.GetPosition2D(new Vector3(5, 5, 0)),
                zoom = 1
            };
        }

        public void UpdateAndDraw(int screenWidth, int screenHeight)
        {
            _cam.offset = new Vector2(screenWidth / 2, screenHeight / 2);

            #region camera controls
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                _cam.target -= GetMouseDelta() / _cam.zoom;

            _cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

            if (_cam.zoom < 0.1f)
                _cam.zoom = 0.1f;

            if (_cam.zoom > 10f)
                _cam.zoom = 10f;
            #endregion

            
            ClearBackground(new Color(10, 130, 255, 255));

            BeginMode2D(_cam);
            _worldDrawer.Draw(_cam);
            EndMode2D();

            #region HUD
            {
                int width = MeasureText("CitySim", 60);
                DrawFPS(10, 10);
                DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
                    60, new Color(0, 0, 0, 100));

                DrawText("CitySim", screenWidth / 2 - width / 2, 20,
                    60, RAYWHITE);

                RaylibExtensions.MyDrawRect(screenWidth - 210, 0, screenWidth, screenHeight,
                    new Color(0, 0, 0, 100));


                int currentY = 10;

                for (int i = 0; i < _worldDrawer.OverlayNames.Count; i++)
                {
                    bool enabled = _worldDrawer.IsOverlayEnabled(i);

                    enabled = GuiCheckBox(
                        new Rectangle(screenWidth - 200, currentY, 20, 20),
                        _worldDrawer.OverlayNames[i], enabled);

                    _worldDrawer.ToggleOverlay(i, enabled);

                    currentY += 50;
                }
            }
            #endregion
        }
    }
}
