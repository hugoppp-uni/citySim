using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using CitySim.Backend.Entity.Agents;

namespace CitySim.Frontend
{
    internal class CitySimView
    {
        private WorldDrawer _worldDrawer;
        private Camera2D _cam;
        private readonly Backend.CitySim _model;

        private Person? _selectedPerson = null;

        public CitySimView(Backend.CitySim model)
        {
            _worldDrawer = new WorldDrawer(model);

            _cam = new()
            {
                target = _worldDrawer.Grid.GetPosition2D(new Vector3(5, 5, 0)),
                zoom = 1
            };
            _model = model;
        }

        private void UpdateCamera()
        {
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                _cam.target -= GetMouseDelta() / _cam.zoom;

            _cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

            if (_cam.zoom < 0.1f)
                _cam.zoom = 0.1f;

            if (_cam.zoom > 10f)
                _cam.zoom = 10f;
        }

        private void HandleClick()
        {
            if (_worldDrawer.HoveredPerson is not null)
                _selectedPerson = _worldDrawer.HoveredPerson;
            else
                _selectedPerson = null;
        }

        private void DrawHud(int screenWidth, int screenHeight)
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

        public void UpdateAndDraw(int screenWidth, int screenHeight)
        {
            //bit hacky but who cares
            bool clicked = GetGestureDragVector() == Vector2.Zero && 
                GuiLabelButton(new Rectangle(0, 0, screenWidth, screenHeight), 
                string.Empty);

            _cam.offset = new Vector2(screenWidth / 2, screenHeight / 2);

            UpdateCamera();

            ClearBackground(new Color(10, 130, 255, 255));

            BeginMode2D(_cam);
            _worldDrawer.Draw(_cam);
            EndMode2D();




            if (clicked)
                HandleClick();

            if (_selectedPerson is not null)
            {
                //draw indicator (animated arrow, pointing down to and hovering over the selected person)

                var pos = GetWorldToScreen2D(
                    _worldDrawer.GetPersonPosition2D(_selectedPerson),
                    _cam) + new Vector2(0, -WorldDrawer.GetPersonVisualHeight(_cam));

                pos.Y += -20 +(float)Math.Sin(GetTime()*2) * 10;

                var col = ColorFromHSV(190 - (float)Math.Sin(GetTime() * 2) * 10, 1, 1);

                unsafe
                {
                    var _pos = pos + new Vector2(2, 2);
                    DrawTriangle(
                        _pos+new Vector2( 15, -20),
                        _pos+new Vector2(-15, -20),
                        _pos,
                        new Color(0, 0, 0, 150)
                        );
                    _pos = pos;
                    DrawTriangle(
                        _pos + new Vector2(15, -20),
                        _pos + new Vector2(-15, -20),
                        _pos,
                        col
                        );
                }
            }


            DrawHud(screenWidth, screenHeight);
        }
    }
}
