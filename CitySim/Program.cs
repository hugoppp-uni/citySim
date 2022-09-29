using CitySim.Agents;
using CitySim.World;
using Mars.Components.Starter;
using Mars.Core.Simulation;
using Mars.Interfaces.Model;
using NetTopologySuite.Algorithm;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

// See https://aka.ms/new-console-template for more information
Console.WriteLine("Hello, World!");

var desc = new ModelDescription();
desc.AddLayer<GridWorld>();
desc.AddAgent<Person, GridWorld>();

var config = new SimulationConfig
{
    SimulationIdentifier = "CitySim",
    ModelDescription = desc,
    Globals = new Globals
    {
        StartPoint = DateTime.Now,
        EndPoint = DateTime.Now.AddHours(1),
        DeltaTUnit = TimeSpanUnit.Seconds,
        ShowConsoleProgress = true,
        OutputTarget = OutputTargetType.Csv,
    }
};

var app = SimulationStarter.BuildApplication(desc, config);

var simulation = app.Resolve<ISimulation>();

simulation.StartSimulationAsync(desc, config);






// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 800;
const int screenHeight = 450;

InitWindow(screenWidth, screenHeight, "CitySim");

SetTargetFPS(60);

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing();

    ClearBackground(new Color(10,130,255,255));


    for (int x = 0; x < 100; x++)
    {
        for (int y = 0; y < 100; y++)
        {
            if(Vector2.Distance(new Vector2(x,y), new Vector2(8, 6))<5)
                DrawRectangleRounded(new Rectangle(x * 50-2, y * 30-2, 50+4, 30 + 4), 0.2f, 3,
                    new Color(30, 80, 0, 255));
        }
    }

    for (int x = 0; x < 100; x++)
    {
        for (int y = 0; y < 100; y++)
        {
            if (Vector2.Distance(new Vector2(x, y), new Vector2(8, 6)) < 5)
                DrawRectangleRounded(new Rectangle(x * 50 + 1, y * 30 + 1, 50 - 2, 30 - 2), 0.1f, 3,
                    new Color(50, 255, 0, 255));
        }
    }

    int width = MeasureText("CitySim", 60);

    DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
        60, new Color(0, 0, 0, 100));

    DrawText("CitySim", screenWidth / 2 - width / 2, 20,
        60, RAYWHITE);

    EndDrawing();
}

CloseWindow();
