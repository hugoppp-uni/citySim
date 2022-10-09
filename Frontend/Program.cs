using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

void MyDrawRect(int left, int top, int right, int bottom, Color color)
{
    DrawRectangle(left, top, right - left, bottom - top, color);
}

void MyDrawRoundedRect(int left, int top, int right, int bottom, float radius, Color color)
{
    int width = right - left;
    int height = bottom - top;

    //raylib decided to handle roundness in a pretty annoying way
    float roundness = radius / (Math.Min(width, height) / 2);

    DrawRectangleRounded(new(left, top, width, height), roundness, 0,color);
}

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

Vector2 TEST_ISLAND_CENTER = new(
    (float)citySim.GridLayer.GridEnvironment.Centre.X,
    (float)citySim.GridLayer.GridEnvironment.Centre.Y
    );

Camera2D cam = new Camera2D
{
    target = TEST_ISLAND_CENTER*new Vector2(50,30),
    offset = new Vector2(screenWidth/2,screenHeight/2),
    zoom = 2
};

const int TILE_WIDTH = 50;
const int TILE_HEIGHT = 30;


SetTargetFPS(60);

var model = LoadModel("C:\\Users\\jupah\\source\\repos\\Veldrid.PBR\\modules\\glTF-Sample-Models\\2.0\\Duck\\glTF\\Duck.gltf");

// Main game loop
while (!WindowShouldClose())
{
    if(IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        cam.target -= GetMouseDelta()/cam.zoom;

    cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

    BeginDrawing();

    ClearBackground(new Color(10, 130, 255, 255));

    bool IsGround(int tileX, int tileY)
    {
        return true; 
        Vector2 pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

        return Vector2.Distance(pos, TEST_ISLAND_CENTER) < 5;
    }

    bool IsRoad(int tileX, int tileY) => (tileX % 4 == 0 || tileY % 3 == 0) && IsGround(tileX,tileY);


    BeginMode2D(cam);
    for (int x = 0; x < citySim.GridLayer.GridEnvironment.GridWidth; x++)
    {
        for (int y = 0; y < citySim.GridLayer.GridEnvironment.GridHeight; y++)
        {
            if (!IsGround(x,y))
                continue;

            int left = x * TILE_WIDTH;
            int top = y * TILE_HEIGHT;
            int right = left + TILE_WIDTH;
            int bottom = top + TILE_HEIGHT;
            

            Color color = new Color(30, 200, 0, 255);
            MyDrawRoundedRect(
                left - 4, top - 4, 
                right + 4, bottom + 4,
                4, color);
        }
    }

    for (int x = 0; x < citySim.GridLayer.GridEnvironment.GridWidth; x++)
    {
        for (int y = 0; y < citySim.GridLayer.GridEnvironment.GridHeight; y++)
        {
            if (!IsGround(x, y))
                continue;

            int left = x * TILE_WIDTH;
            int top = y * TILE_HEIGHT;
            int right = left + TILE_WIDTH;
            int bottom = top + TILE_HEIGHT;

            var personOnCoord = citySim.GridLayer.GridEnvironment.Explore(x, y, 0).Any();
            Color color = new Color(50, 255, 0, 255);
            MyDrawRoundedRect(
                left + 1, top + 1,
                right - 1, bottom - 1, 1,
                color);

            if (IsRoad(x, y))
            {
                void DrawConnectedRoad(int indX, int indY, Color color)
                {
                    MyDrawRect(left + indX, top + indY, right - indX, bottom - indY, color);

                    if (IsRoad(x - 1, y))
                        MyDrawRect(left, top + indY, left + indX, bottom - indY, color);

                    if (IsRoad(x + 1, y))
                        MyDrawRect(right - indX, top + indY, right, bottom - indY, color);

                    if (IsRoad(x, y - 1))
                        MyDrawRect(left + indX, top, right - indX, top + indY, color);

                    if (IsRoad(x, y + 1))
                        MyDrawRect(left + indX, bottom - indY, right - indX, bottom, color);
                }

                DrawConnectedRoad(10, 5, new Color(150, 150, 150, 255));
                DrawConnectedRoad(13, 7, new Color(90, 90, 90, 255));
                
            }
            else //building
            {
                

                color = new Color(220, 220, 220, 255);
                MyDrawRect(left + 10, top - 5, right - 10, bottom - 5, color);

                color = RAYWHITE;
                MyDrawRect(left + 10, top - 10, right - 10, top + 5, color);
            }

            
        }
    }
    EndMode2D();

    
    {
        int width = MeasureText("CitySim", 60);
        DrawFPS(10, 10);
        DrawText("CitySim", screenWidth / 2 - width / 2 + 2, 22,
            60, new Color(0, 0, 0, 100));

        DrawText("CitySim", screenWidth / 2 - width / 2, 20,
            60, RAYWHITE);
    }
    
    EndDrawing();
}

CloseWindow();