using CitySim.Frontend;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

void MyDrawRect(float left, float top, float right, float bottom, Color color)
{
    DrawRectangleRec(new(left, top, right - left, bottom - top), color);
}

void MyDrawRoundedRect(float left, float top, float right, float bottom, float radius, Color color)
{
    float width = right - left;
    float height = bottom - top;

    //raylib decided to handle roundness in a pretty annoying way
    float roundness = radius / (Math.Min(width, height) / 2);

    DrawRectangleRounded(new(left, top, width, height), roundness, 6,color);
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


SetTargetFPS(60);

//var model = LoadModel("C:\\Users\\jupah\\source\\repos\\Veldrid.PBR\\modules\\glTF-Sample-Models\\2.0\\Duck\\glTF\\Duck.gltf");

SpriteSheet terrainSheet = SpriteSheet.FromPNG_XML(
    Path.Combine("Assets", "landscapeTiles_sheet.png"),
    Path.Combine("Assets", "landscapeTiles_sheet.xml"));

IsoMetricGrid grid = new IsoMetricGrid(128, 64, 32);

Camera2D cam = new Camera2D
{
    target = grid.GetPosition2D(new Vector3(5,5,0)),
    offset = new Vector2(screenWidth / 2, screenHeight / 2),
    zoom = 1
};

Dictionary<byte, int> roadMap = new Dictionary<byte, int>()
{
    //connections bitfield: Y, X, -Y, -X
    {0b_1010, 74},
    {0b_0101, 82},
    {0b_1111, 90},
    {0b_1101, 89},
    {0b_0111, 96},
    {0b_1011, 97},
    {0b_1110, 104},
    {0b_0010, 105},
    {0b_0001, 111},
    {0b_1000, 112},
    {0b_0100, 117},
    {0b_0011, 123},
    {0b_0110, 125},
    {0b_1001, 126},
    {0b_1100, 127},
};

// Main game loop
while (!WindowShouldClose())
{
    if(IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT))
        cam.target -= GetMouseDelta()/cam.zoom;

    

    cam.zoom *= 1 + 0.1f * GetMouseWheelMove();

    if (cam.zoom < 0.1f)
        cam.zoom = 0.1f;

    if (cam.zoom > 10f)
        cam.zoom = 10f;


    BeginDrawing();

    ClearBackground(new Color(10, 130, 255, 255));

    void DrawTerrainTile(int tile, Vector2 bottomCenter)
    {
        string name = $"landscapeTiles_{tile:000}.png";
        var rect = terrainSheet.Rects[name];
        terrainSheet.DrawSprite(name,
            bottomCenter-new Vector2(rect.width/2, rect.height-64));
    }

    

    bool InCityBounds(int tileX, int tileY) =>
        InCityBoundsX(tileX) &&
        InCityBoundsY(tileY);

    bool InCityBoundsX(int tileX) => tileX > 0 && tileX < citySim!.GridLayer.GridEnvironment.DimensionX;
    bool InCityBoundsY(int tileY) => tileY > 0 && tileY < citySim!.GridLayer.GridEnvironment.DimensionY;

    bool IsGround(int tileX, int tileY)
    {
        Vector2 pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

        return Vector2.Distance(pos, new Vector2(202,40)) < 205;
    }

    bool IsRoad(int tileX, int tileY) => (
        (tileX % 4 == 0 && InCityBoundsX(tileX)) || 
        (tileY % 3 == 0 && InCityBoundsY(tileY))
        ) && IsGround(tileX,tileY);


    BeginMode2D(cam);

    foreach (var (cell_x, cell_y, position2d, cell_height) in grid.GetVisibleCells(cam))
    {
        if (!IsGround(cell_x, cell_y))
            continue;

        //connections bitfield: Y, X, -Y, -X

        byte connections = 0;

        if (IsRoad(cell_x, cell_y))
        {
            if(IsRoad(cell_x, cell_y+1))
                connections|=0b1000;
            if (IsRoad(cell_x + 1, cell_y))
                connections |= 0b0100;
            if (IsRoad(cell_x, cell_y - 1))
                connections |= 0b0010;
            if (IsRoad(cell_x - 1, cell_y))
                connections |= 0b0001;

            DrawTerrainTile(roadMap[connections], position2d);
        }
        else
            DrawTerrainTile(67, position2d);

        if (citySim.GridLayer.GridEnvironment.Explore(cell_x, cell_y, 0).Any())
        {
            //string text = "P";

            //int width = MeasureText(text, 40);
            //DrawText(text, position2d.X - width-1, position2d.Y-40, 40, new Color(0,0,0,50));
            //DrawText(text, position2d.X - width+0.5f, position2d.Y-40+0.5f, 40, WHITE);

            Matrix4x4 modelView = RlGl.rlGetMatrixModelview();

            Vector2 pos = position2d - new Vector2(1, 1);

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, new Color(0, 0, 0, 50));
            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, new Color(0, 0, 0, 50));

            pos = position2d;

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, WHITE);
            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, WHITE);
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