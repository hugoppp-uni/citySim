using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util.Learning;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Entity;

namespace CitySim.Frontend
{
    internal unsafe class CitySimView
    {
        private WorldDrawer _worldDrawer;
        private Camera2D _cam;
        private readonly Backend.CitySim _model;

        private IPositionableEntity? _selectedEntity = null;

        private object? _hoveredElement = null;
        private object? _dragStartElement = null;

        private PersonInfoView? _personInfoView;

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
            if (_dragStartElement==_worldDrawer && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
                _cam.target -= GetMouseDelta() / _cam.zoom;

            if (_hoveredElement == _worldDrawer)
                _cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

            if (_cam.zoom < 0.1f)
                _cam.zoom = 0.1f;

            if (_cam.zoom > 10f)
                _cam.zoom = 10f;
        }

        private void HandleClick()
        {
            _selectedEntity = null;

            if (_worldDrawer.HoveredEntity is not null)
                _selectedEntity = _worldDrawer.HoveredEntity;
        }

        private void DrawHud(int screenWidth, int screenHeight, ref object? newHoveredElement)
        {
            DrawFPS(10, 10);
            DrawText($"Average model training duration: {ModelWorker.GetInstance(nameof(Person)).AverageFitDuration} ms",
                10, 25,15, BLACK);

            {
                //Title
                int width = MeasureText("CitySim", 60);
                DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
                    60, new Color(0, 0, 0, 100));

                DrawText("CitySim", screenWidth / 2 - width / 2, 20,
                    60, RAYWHITE);
            }

            int optionsPanelWidth = 210;
            int infoPanelHeight = 300;

            Color panelColor = new Color(0, 0, 0, 100);

            var mousePos = GetMousePosition();

            {
                //Options panel
                
                
                int left = screenWidth - optionsPanelWidth;
                int top = 0;
                int right = screenWidth;
                int bottom = screenHeight;

                if (left <= mousePos.X && mousePos.X <= right &&
                    top <= mousePos.Y && mousePos.Y <= bottom)
                    newHoveredElement = null;


                RaylibExtensions.MyDrawRect(left, top, right, bottom,
                    panelColor);

                int currentY = 10;

                for (int i = 0; i < _worldDrawer.OverlayNames.Count; i++)
                {
                    bool enabled = _worldDrawer.IsOverlayEnabled(i);

                    enabled = GuiCheckBox(
                        new Rectangle(screenWidth - optionsPanelWidth + 10, currentY, 20, 20),
                        _worldDrawer.OverlayNames[i], enabled);

                    _worldDrawer.ToggleOverlay(i, enabled);

                    currentY += 50;
                }
            }

            if (_selectedEntity is Person selectedPerson)
            {
                //Selected person info panel
                var bounds = new Rectangle(0, screenHeight - infoPanelHeight, screenWidth - optionsPanelWidth + 2, infoPanelHeight);
                const int padding = 10;

                var viewBounds = new Rectangle(bounds.X + padding, bounds.Y + padding,
                    bounds.width - 2 * padding, bounds.height - 2 * padding);

                if (_personInfoView?.Person != _selectedEntity)
                {
                    _personInfoView = new PersonInfoView(selectedPerson, (0, 0), viewBounds);
                }

                if (CheckCollisionPointRec(mousePos, bounds))
                    newHoveredElement = _personInfoView;

                DrawRectangleRec(bounds, panelColor);

                _personInfoView.ViewBounds = viewBounds;

                _personInfoView.UpdateAndDraw(_hoveredElement == _personInfoView);
            }
        }

        public void UpdateAndDraw(int screenWidth, int screenHeight)
        {
            object? newHoveredElement = _worldDrawer;

            //bit hacky but who cares
            bool clicked = _hoveredElement == _worldDrawer &&
                GetGestureDragVector() == Vector2.Zero && 
                GuiLabelButton(new Rectangle(0, 0, screenWidth, screenHeight), 
                string.Empty);

            if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
                _dragStartElement = _hoveredElement;

            _cam.offset = new Vector2(screenWidth / 2, screenHeight / 2);

            UpdateCamera();

            ClearBackground(new Color(10, 130, 255, 255));

            BeginMode2D(_cam);
            _worldDrawer.Draw(_cam, _hoveredElement==_worldDrawer);
            EndMode2D();




            if (clicked)
                HandleClick();

            if (_selectedEntity is Person selectedPerson)
            {
                //draw indicator (animated arrow, pointing down to and hovering over the selected person)

                var pos = GetWorldToScreen2D(
                    _worldDrawer.GetPersonPosition2D(selectedPerson),
                    _cam) + new Vector2(0, -WorldDrawer.GetPersonVisualHeight(_cam));

                pos.Y += -20 +(float)Math.Sin(GetTime()*2) * 10;

                var col = ColorFromHSV(190 - (float)Math.Sin(GetTime() * 2) * 10, 1, 1);


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

            if (_selectedEntity is House selectedHouse)
            {
                //draw indicator (animated arrow, pointing down to and hovering over the selected person)

                var pos = GetWorldToScreen2D(
                    _worldDrawer.Grid.GetPosition2D(new Vector3(
                        (float)selectedHouse.Position.X,
                        (float)selectedHouse.Position.Y,
                        WorldDrawer.GetHouseBlockHeight()
                        )),
                    _cam);

                pos.Y += -20 + (float)Math.Sin(GetTime() * 2) * 10;

                var col = ColorFromHSV(160 - (float)Math.Sin(GetTime() * 2) * 10, 1, 1);


                var _pos = pos + new Vector2(2, 2);
                DrawTriangle(
                    _pos + new Vector2(15, -20),
                    _pos + new Vector2(-15, -20),
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


            DrawHud(screenWidth, screenHeight, ref newHoveredElement);

            _hoveredElement = newHoveredElement;
        }
    }
}
