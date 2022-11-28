using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Util.Learning;
using CitySim.Backend.Entity.Structures;
using CitySim.Backend.Entity;
using CitySim.Backend.World;
using CitySim.Frontend.HUD.EntityInfoViews;
using CitySim.Frontend.Helpers;

namespace CitySim.Frontend
{
    internal unsafe class CitySimView
    {
        private readonly WorldDrawer _worldDrawer;
        private Camera2D _cam;
        private readonly Backend.CitySim _model;
        private readonly EventLogEntry[] _eventLog = new EventLogEntry[EventLog.Capacity];
        private readonly Font _defaultFont;

        private IPositionableEntity? _selectedEntity = null;
        private const int OPTIONS_PANEL_WIDTH = 210;
        private object? _hoveredElement = null;
        private object? _dragStartElement = null;

        private IEntityInfoView? _activeInfoView;

        public CitySimView(Backend.CitySim model)
        {
            _defaultFont = LoadFont(Path.Combine("Assets", "arial.ttf"));
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
            if (_dragStartElement == _worldDrawer && IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
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

        private void DrawStats(int screenWidth, int screenHeight)
        {
            var personCount = _model.WorldLayer.GridEnvironment.Entities.OfType<Person>().Count();
            var statesWidth = 150;
            var x = screenWidth - OPTIONS_PANEL_WIDTH - statesWidth;
            var y = 10;
            var houses = _model.WorldLayer.Structures.OfType<House>().Sum(house => house.MaxSpaces);
            var restaurants = _model.WorldLayer.Structures.OfType<Restaurant>()
                .Sum(restaurant => restaurant.MaxCapacityPerTick);
            DrawText( $"Person: {personCount}", x, y, 16, WHITE);
            DrawText( $"Housing capacity: {houses}", x, y + 20, 16, WHITE);
            DrawText( $"Restaurants: {restaurants}", x, y + 40, 16, WHITE);
        }
        
        
        private void DrawText( string text,
            int posX,
            int posY,
            int fontSize,
            Color color)
        {
            DrawTextEx(_defaultFont, text, new Vector2{ X = posX, Y = posY },
                fontSize, 1, color);
        }

        private void DrawHud(int screenWidth, int screenHeight, ref object? newHoveredElement)
        {
            DrawFPS(10, 10);
            DrawEventLog();
            DrawStats(screenWidth, screenHeight);
            
            DrawText($"Average model training duration: {ModelWorker.GetInstance(nameof(Person)).AverageFitDuration} ms",
                10, 25,15, WHITE);
            DrawText($"Current tick: {_model.WorldLayer.GetCurrentTick()}", 10, screenHeight - 16, 
                16, WHITE);

            {
                //Title
                int width = MeasureText("CitySim", 60);
                DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
                    60, new Color(0, 0, 0, 100));

                DrawText("CitySim", screenWidth / 2 - width / 2, 20,
                    60, RAYWHITE);
            }

            int infoPanelHeight = 300;

            Color panelColor = new Color(0, 0, 0, 100);

            var mousePos = GetMousePosition();

            {
                //Options panel
                int left = screenWidth - OPTIONS_PANEL_WIDTH;
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
                        new Rectangle(screenWidth - OPTIONS_PANEL_WIDTH + 10, currentY, 20, 20),
                        _worldDrawer.OverlayNames[i], enabled);

                    _worldDrawer.ToggleOverlay(i, enabled);

                    currentY += 50;
                }
                GuiLabel(new Rectangle(screenWidth - OPTIONS_PANEL_WIDTH + 10, currentY, OPTIONS_PANEL_WIDTH - 30, 20),
                    "Ticks per second");
                currentY += 30;
                _model.SimulationController.TicksPerSecond = (int) Math.Floor(Math.Pow(2,GuiSlider(
                    new Rectangle(screenWidth - OPTIONS_PANEL_WIDTH + 10, currentY, OPTIONS_PANEL_WIDTH - 30, 20), "",
                    _model.SimulationController.TicksPerSecond.ToString(), (float)Math.Log2(_model.SimulationController.TicksPerSecond),0,10)));
                currentY += 30;
                var pausedClicked = GuiButton(new Rectangle(screenWidth - OPTIONS_PANEL_WIDTH + 10, currentY, 50, 20),
                    _model.SimulationController.Paused ? "Continue" :"Pause");
                
                if (pausedClicked)
                {
                    _model.SimulationController.Paused = !_model.SimulationController.Paused;
                }

                if (_model.SimulationController.Paused)
                {
                    var oneStep = GuiButton(new Rectangle(screenWidth - OPTIONS_PANEL_WIDTH + 70, currentY, 50, 20),
               "Step");
                    if (oneStep)
                    {
                        _model.SimulationController.ContinueEvent.Set();
                    }
                }

                currentY += 30;
            }

            if(_selectedEntity is not null)
            {
                //Selected entity info panel
                var bounds = new Rectangle(0, screenHeight - infoPanelHeight, screenWidth - OPTIONS_PANEL_WIDTH + 2, infoPanelHeight);
                const int padding = 10;

                var viewBounds = new Rectangle(bounds.X + padding, bounds.Y + padding,
                    bounds.width - 2 * padding, bounds.height - 2 * padding);

                
                if(_activeInfoView?.Entity != _selectedEntity)
                {
                    _activeInfoView = _selectedEntity switch
                    {
                        Person x     => new PersonInfoView(    x, (0, 0), viewBounds),
                        House x      => new HouseInfoView(     x, (0, 0), viewBounds),
                        Restaurant x => new RestaurantInfoView(x, (0, 0), viewBounds),
                        _ => null
                    };
                }
                
                if (CheckCollisionPointRec(mousePos, bounds))
                    newHoveredElement = _activeInfoView;

                DrawRectangleRec(bounds, panelColor);

                _activeInfoView!.ViewBounds = viewBounds;

                _activeInfoView!.UpdateAndDraw(_hoveredElement == _activeInfoView);
            }

            
        }

        private void DrawEventLog()
        {
            int n = WorldLayer.Instance.EventLog.WriteToArray(_eventLog);

            for (var index = 0; index < n; index++)
            {
                var eventLogEntry = _eventLog[index];
                const int showEventsInLastNTicks = 5;
                if (eventLogEntry.Tick < WorldLayer.CurrentTick - showEventsInLastNTicks)
                    break;

                var color = WorldDrawer.GetPersonColor(eventLogEntry.Person);
                DrawText(eventLogEntry.Person.Name + ": " + eventLogEntry.Log,
                    30, index * 16 + 55, 14,
                    color);
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
            _worldDrawer.Draw(_cam, _hoveredElement == _worldDrawer);
            EndMode2D();


            if (clicked)
                HandleClick();

            if (_selectedEntity is Person selectedPerson)
            {
                //draw indicator (animated arrow, pointing down to and hovering over the selected person)

                var pos = GetWorldToScreen2D(
                    _worldDrawer.GetPersonPosition2D(selectedPerson),
                    _cam) + new Vector2(0, -WorldDrawer.GetPersonVisualHeight(_cam));

                pos.Y += -20 + (float)Math.Sin(GetTime() * 2) * 10;

                var col = ColorFromHSV(190 - (float)Math.Sin(GetTime() * 2) * 10, 1, 1);


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