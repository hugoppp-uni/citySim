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

SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_MSAA_4X_HINT);
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

CitySimView view = new(citySim);

RayGui.GuiSetStyle((int)RaylibExtensions.GuiControl.DEFAULT, (int)RaylibExtensions.GuiControlProperty.TEXT_COLOR_NORMAL,
    ColorToInt(WHITE));

Camera2D cam = new Camera2D()
{
    zoom = 1
};

int Circle_VertexCount = 128;

Vector2[] headVerts = new Vector2[Circle_VertexCount + 2];
Vector2[] headVertsDynamic = new Vector2[Circle_VertexCount + 2];

headVerts[0] = new Vector2(0, -100);

for (int i = 0; i < Circle_VertexCount + 1; i++)
{
    float a = MathF.PI * 2 / Circle_VertexCount * i;
    var (sin, cos) = MathF.SinCos(a);
    headVerts[i + 1] = headVerts[0] + new Vector2(sin, cos) * 50;
}


float RowHeight = 2f;

float BodyTop = -40;
float BodyBottom = 40;

float BodyCornerRadius = 10;

int Rows = (int)((BodyBottom - BodyTop) / RowHeight);

Vector2[] bodyVerts = new Vector2[Rows*2 + 2];
Vector2[] bodyVertsDynamic = new Vector2[Rows*2 + 2];

for (int i = 0; i < Rows+1; i++)
{
    float t;

    float y = BodyTop + RowHeight * i;
    if (y < BodyTop + BodyCornerRadius)
        t = 1-(y - BodyTop)/ BodyCornerRadius;

    else if (y > BodyBottom - BodyCornerRadius)
        t = 1-(BodyBottom-y) / BodyCornerRadius;
    else
        t = 0;

    float d = 1-MathF.Sqrt(1 - t * t);

    bodyVerts[i * 2] = new Vector2(50 - d * BodyCornerRadius, y);
    bodyVerts[i * 2 + 1] = new Vector2(-50 + d * BodyCornerRadius, y);

}

// Main game loop
while (!WindowShouldClose())
{
    BeginDrawing();
    ClearBackground(BLACK);
    //view.UpdateAndDraw(GetScreenWidth(), GetScreenHeight());

    cam.offset = new Vector2(GetScreenWidth(), GetScreenHeight()) / 2;


    static float EaseOutElastic(float x)
    {
        float c4 = (2 * MathF.PI) / 3;

        return x == 0
          ? 0
          : x == 1
          ? 1
          : MathF.Pow(2, -10 * x) * MathF.Sin((x * 10 - 0.75f) * c4) + 1;
    }

    double time = GetTime() - 2;

    float pealProgress = MathF.Min((float)time, 1);

    float splitProgress = Math.Clamp(((float)time - 1), 0, 0.6f) / 0.6f;

    float jumpProgress = Math.Clamp((float)time - 1.6f, 0, 0.4f) / 0.4f;

    float textFade = Math.Clamp((float)time - 1.8f, 0, 0.6f) / 0.6f;


    float splitAnim = EaseOutElastic(splitProgress);

    BeginMode2D(cam);

    unsafe
    {
        Vector2 Func(float y)
        {
            float headTop = -150;

            float t_y = (y - BodyBottom) / (BodyBottom - headTop);

            float t1 = pealProgress;
            float t2 = splitAnim;
            float t3 = jumpProgress;

            float t = (float)time;



            float a = t_y * t_y * t1 * (50 + MathF.Sin(t * 50) * 4);
            float b = 0;

            return new Vector2(
                a * (1 - t2) + b * t2,
                (-(t3 * t3) + 2 * t3) * -30);
        }


        Vector2 posA = Vector2.Lerp(
            new Vector2(0, 0),
            new Vector2(0, 0),
            splitAnim);

        Vector2 posB = Vector2.Lerp(
            new Vector2(0, 0),
            new Vector2(120, 160),
            splitAnim);



        Font font = GetFontDefault();

        float fontSize = 60;

        var size = MeasureTextEx(font, "CitySim", fontSize, fontSize / font.baseSize);

        var col = ColorAlpha(WHITE, textFade);

        DrawText("CitySim", 0 - size.X / 2, 60 - size.Y / 2, fontSize, col);

        for (int i = 0; i < headVertsDynamic.Length; i++)
        {
            headVertsDynamic[i] = headVerts[i] +
                Func(headVerts[i].Y) * new Vector2(-1, 1) + posA;
        }

        fixed (Vector2* hvp = headVertsDynamic)
        {
            DrawTriangleFan(hvp, headVerts.Length, WHITE);
        }


        for (int i = 0; i < headVertsDynamic.Length; i++)
        {
            headVertsDynamic[i] = headVerts[i] +
                Func(headVerts[i].Y) * new Vector2(1, 1) + posB;
        }

        fixed (Vector2* hvp = headVertsDynamic)
        {
            DrawTriangleFan(hvp, headVerts.Length, WHITE);
        }



        for (int i = 0; i < bodyVertsDynamic.Length; i++)
        {
            bodyVertsDynamic[i] = bodyVerts[i] +
                Func(bodyVerts[i].Y) * new Vector2(-1, 1) + posA;
        }

        fixed (Vector2* bvp = bodyVertsDynamic)
        {
            DrawTriangleStrip(bvp, bodyVerts.Length, WHITE);
        }



        for (int i = 0; i < bodyVertsDynamic.Length; i++)
        {
            bodyVertsDynamic[i] = bodyVerts[i] +
                Func(bodyVerts[i].Y) * new Vector2(1, 1) + posB;
        }

        fixed (Vector2* bvp = bodyVertsDynamic)
        {
            DrawTriangleStrip(bvp, bodyVerts.Length, WHITE);
        }
    }

    EndMode2D();

    EndDrawing();
}

CloseWindow();