using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Agents;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;

Console.WriteLine("Hello, World!");




//change simulation speed


// Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 800;
const int screenHeight = 450;

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

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing(); 

    ClearBackground(new Color(10, 130, 255, 255));


    var personOnCoord = citySim.WorldLayer.GridEnvironment.Entities.OfType<Person>().ToDictionary(person => ((int)person.Position.X, (int)person.Position.Y));
    var buildingOnCoord =
        citySim.WorldLayer.GridEnvironment.Entities.OfType<Structure>().ToDictionary(structure => ((int)structure.Position.X, (int)structure.Position.Y));
    
    for (int x = 0; x < 10; x++)
    {
        for (int y = 0; y < 10; y++)
        {
            Color color = personOnCoord.ContainsKey((x,y)) ? new Color(50, 255, 0, 255) : 
                buildingOnCoord.ContainsKey((x,y)) ? new Color(255, 255, 255, 255) :
                new Color(50, 255, 255, 255);
            DrawRectangleRounded(new Rectangle(x * 50 + 150, y * 30 + 100, 50 - 2, 30 - 2), 0.1f, 3,
                color);
        }
    }

    int width = MeasureText("CitySim", 60);
    DrawFPS(10, 10);
    DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
        60, new Color(0, 0, 0, 100));

    DrawText("CitySim", screenWidth / 2 - width / 2, 20,
        60, RAYWHITE);

    EndDrawing();
}

CloseWindow();