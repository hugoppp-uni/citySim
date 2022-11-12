using CitySim.Frontend;
using Raylib_CsLo;
using System.Numerics;
using System.Security.Policy;
using static Raylib_CsLo.Raylib;

Console.WriteLine("Hello, World!");




//change simulation speed


// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 1400;
const int screenHeight = 900;

SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_MSAA_4X_HINT);
InitWindow(screenWidth, screenHeight, "CitySim");

SetTargetFPS(60);


var citySim = new CitySim.Backend.CitySim
{
    SimulationController =
    {
        TicksPerSecond = 2
    }
};

Task simulationTask;
bool isSimulationRunning = false;


CitySimView view = new(citySim);

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.TEXT_COLOR_NORMAL,
    ColorToInt(WHITE));

Camera2D cam = new Camera2D()
{
    zoom = 4
};

double startTime = GetTime();

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing();

    double time = GetTime() - startTime;

    float splitProgress = Math.Clamp((float)time, 0, 1.6f) / 1.6f;

    float jumpProgress = Math.Clamp((float)time - 1.6f, 0, 0.4f) / 0.4f;

    float textFade = Math.Clamp((float)time - 1.8f, 0, 0.6f) / 0.6f;
    
    float finalWipe = Math.Clamp((float)time - 2.4f, 0, 0.8f) / 0.8f;


    if (finalWipe == 1 && !isSimulationRunning)
    {
        citySim.StartAsync();
        isSimulationRunning = true;
    }

    if (finalWipe > 0)
        view.UpdateAndDraw(GetScreenWidth(), GetScreenHeight());

    cam.offset = new Vector2(GetScreenWidth(), GetScreenHeight()) / 2;




    float t3 = jumpProgress;

    float y_offset = (-(t3 * t3) + 2 * t3) * -15;

    var posAStart = new Vector2(0, 10);
    var posAEnd = new Vector2(-20, 10 + y_offset);

    var posBStart = new Vector2(0, 10);
    var posBEnd = new Vector2(20, 10 + y_offset);

    BeginScissorMode(0, 0, (int)(GetScreenWidth() * (1 - finalWipe * finalWipe * finalWipe)), GetScreenHeight());
    ClearBackground(BLACK);
    BeginMode2D(cam);



    //Fading in CitySim text
    {
        Font font = GetFontDefault();

        float fontSize = 30;

        var size = MeasureTextEx(font, "CitySim", fontSize, fontSize / font.baseSize);

        var col = ColorAlpha(WHITE, textFade);

        DrawText("CitySim", 0 - size.X / 2, 20 - size.Y / 2, fontSize, col);
    }

    SplittingPersonDrawer.Draw(splitProgress, posAStart, posAEnd, WHITE, WHITE,
                                              posBStart, posBEnd, WHITE, WHITE);

    EndMode2D();
    EndScissorMode();

    EndDrawing();
}

CloseWindow();