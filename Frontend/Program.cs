using CitySim.Frontend;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

Console.WriteLine("Hello, World!");




//change simulation speed


// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 1400;
const int screenHeight = 900;

InitWindow(screenWidth, screenHeight, "CitySim");

var citySim = new CitySim.Backend.CitySim
{
    SimulationController =
    {
        TicksPerSecond = 2
    }
};
var simulationTask = citySim.StartAsync();



SetTargetFPS(60);

WorldDrawer worldDrawer = new(citySim);

Camera2D cam = new()
{
    target = worldDrawer.Grid.GetPosition2D(new Vector3(5,5,0)),
    offset = new Vector2(screenWidth / 2, screenHeight / 2),
    zoom = 1
};

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.TEXT_COLOR_NORMAL,
    ColorToInt(WHITE));

// Main game loop
while (!WindowShouldClose())
{
    #region camera controls
    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        cam.target -= GetMouseDelta()/cam.zoom;

    cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

    if (cam.zoom < 0.1f)
        cam.zoom = 0.1f;

    if (cam.zoom > 10f)
        cam.zoom = 10f;
    #endregion

    BeginDrawing();
    ClearBackground(new Color(10, 130, 255, 255));

    BeginMode2D(cam);
    worldDrawer.Draw(cam);
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

        for (int i = 0; i < worldDrawer.OverlayNames.Count; i++)
        {
            bool enabled = worldDrawer.IsOverlayEnabled(i);

            enabled = RayGui.GuiCheckBox(
                new Rectangle(screenWidth - 200, currentY, 20, 20),
                worldDrawer.OverlayNames[i], enabled);

            worldDrawer.ToggleOverlay(i, enabled);

            currentY += 50;
        }
    }
    #endregion

    EndDrawing();
}

CloseWindow();