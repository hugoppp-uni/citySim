using CitySim.Frontend;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

Console.WriteLine("Hello, World!");


//change simulation speed


// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 1400;
const int screenHeight = 900;

SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_MSAA_4X_HINT);
InitWindow(screenWidth, screenHeight, "CitySim");

var personMindFileName = "./ModelWeights/personMind.hdf5";
var citySim = new CitySim.Backend.CitySim(personMindWeightsFileToLoad: personMindFileName,
    newSaveLocationForPersonMindWeights: personMindFileName, personCount: 10)
{
    SimulationController =
    {
        TicksPerSecond = 2
    }
};
var simulationTask = citySim.StartAsync();


SetTargetFPS(60);

CitySimView view = new(citySim);

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.TEXT_COLOR_NORMAL,
    ColorToInt(WHITE));

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing();
    view.UpdateAndDraw(GetScreenWidth(), GetScreenHeight());
    EndDrawing();
}

citySim.Abort();
CloseWindow();