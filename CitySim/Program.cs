using CitySim.Agents;
using CitySim.World;
using Mars.Components.Starter;
using Mars.Interfaces.Model;
using NetTopologySuite.Algorithm;
using Raylib_CsLo;
using System.Numerics;
using Mars.Core.Simulation;
using Mars.Interfaces.Layers;
using static Raylib_CsLo.Raylib;

var desc = new ModelDescription();
desc.AddLayer<GridLayer>();
desc.AddLayer<FixedUpdateLayer>();
desc.AddAgent<Person, GridLayer>();

var config = new SimulationConfig
{
    SimulationIdentifier = "CitySim",
    ModelDescription = desc,
    Globals = new Globals
    {
        StartPoint = DateTime.Now,
        EndPoint = DateTime.Now.AddSeconds(30),
        DeltaTUnit = TimeSpanUnit.Seconds,
        ShowConsoleProgress = false,
        OutputTarget = OutputTargetType.Csv,
    }
};

var application = SimulationStarter.BuildApplication(desc, config);
var simulation = application.Resolve<ISimulation>();
Task.Factory.StartNew(() =>
{
    var loopResults = simulation.StartSimulation();
    Console.WriteLine($"Simulation execution finished after {loopResults.Iterations} steps");
});

GridLayer gridLayer = (GridLayer)simulation.WorkflowState.Model.Layers[new LayerType(typeof(GridLayer))];

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

    ClearBackground(new Color(10, 130, 255, 255));


    for (int x = 0; x < 10; x++)
    {
        for (int y = 0; y < 10; y++)
        {
            var personOnCoord = gridLayer.GridEnvironment.Explore(x, y, 0).Any();
            Color color = personOnCoord ? new Color(50, 255, 0, 255) : new Color(50, 255,255,255);
            DrawRectangleRounded(new Rectangle(x * 50 + 150, y * 30 + 100, 50 - 2, 30 - 2), 0.1f, 3,
                    color);
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