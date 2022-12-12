using CitySim.Backend;
using CitySim.Frontend;
using CitySim.Frontend.Helpers;
using Raylib_CsLo;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using CitySim.Backend.Entity.Agents.Behavior;
using Plugins;
using static Raylib_CsLo.Raylib;

//change simulation speed


// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 1400;
const int screenHeight = 900;

SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_MSAA_4X_HINT);
InitWindow(screenWidth, screenHeight, "CitySim");

List<Type> FindMindImplementations()
{
    return Assembly.LoadFrom("Plugins.dll")
        .GetTypes()
        .Where(p => typeof(IMind).IsAssignableFrom(p))
        .Where(p => p != typeof(MindMock) && p != typeof(IMind) && p != typeof(ExampleUserMind))
        .Concat(new []{typeof(PersonMind)})
        .ToList();
}


CitySim.Backend.CitySim CreateModel(Type mindImpl)
{
    string? personMindFileName = "./ModelWeights/personMind.hdf5";
    return new CitySim.Backend.CitySim(
        personMindWeightsFileToLoad: personMindFileName,
        newSaveLocationForPersonMindWeights: personMindFileName,
        personCount: 30,
        personMindBatchSize: x => x / 2,
        personMindLearningRate: 0.02f,
    personActionExplorationRate:0,
        training: true,
        mindImplementationType: mindImpl
    )
    {
        SimulationController =
        {
            TicksPerSecond = 2
        }
    };
}

Task simulationTask;
bool isSimulationRunning = false;
bool isModelCreated = false;

SetTargetFPS(60);

CitySimView? view = null;
CitySim.Backend.CitySim? citySim = null;

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.TEXT_COLOR_NORMAL,
    ColorToInt(WHITE));

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.BASE_COLOR_NORMAL,
    ColorToInt(DARKGRAY));

Camera2D cam = new Camera2D()
{
    zoom = 4
};

double startTime = GetTime();

double finalWipeStartTime = double.MaxValue;

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing();

    double time = GetTime() - startTime;

    if (isModelCreated && finalWipeStartTime == double.MaxValue)
        finalWipeStartTime = time;


    float splitProgress = Math.Clamp((float)time, 0, 1.6f) / 1.6f;

    float jumpProgress = Math.Clamp((float)time - 1.6f, 0, 0.4f) / 0.4f;

    float textFade = Math.Clamp((float)time - 1.8f, 0, 0.6f) / 0.6f;

    float finalWipe = Math.Clamp((float)(time - finalWipeStartTime), 0, 0.8f) / 0.8f;


    view?.UpdateAndDraw(GetScreenWidth(), GetScreenHeight());

    if (finalWipe == 1 && !isSimulationRunning)
    {
        citySim!.StartAsync();
        isSimulationRunning = true;
    }

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


    if (!isModelCreated)
    {
        Type? choice = null;


        var mindImplementations = FindMindImplementations();


        int y = -mindImplementations.Count*50 - 30;


        {
            string text = "Choose your Mind";

            Font font = GetFontDefault();

            float fontSize = 20;

            var size = MeasureTextEx(font, text, fontSize, fontSize / font.baseSize);

            var col = GRAY;

            DrawText(text, 
                GetScreenWidth() / 2 - size.X / 2,
                GetScreenHeight() + y - size.Y - 40, fontSize, col);
        }


        int width = GetScreenWidth() / 2;

        int default_size = RayGui.GuiGetStyle((int)RaylibExtensions.GuiControl.DEFAULT,
            (int)RaylibExtensions.GuiControlProperty.TEXT_SIZE);

        RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT,
            (int)RaylibExtensions.GuiControlProperty.TEXT_SIZE, 22);

        foreach (var mindType in mindImplementations)
        {
            var mindName = mindType == typeof(PersonMind) ? "AI Mind" : mindType.Name;
            
            if (RayGui.GuiButton(new Rectangle(
                        GetScreenWidth() / 2 - width/2, 
                        GetScreenHeight() + y, width, 40), 
                    mindName))
                choice = mindType;

            y += 50;
        }

        RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT,
            (int)RaylibExtensions.GuiControlProperty.TEXT_SIZE, default_size);


        if (choice is not null)
        {
            citySim = CreateModel(choice);
            view = new CitySimView(citySim);

            isModelCreated = true;
        }
    }

    EndDrawing();
}

citySim!.Abort();
CloseWindow();