using Raylib_CsLo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Tensorflow;
using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend
{
    internal static class GraphDebugProgram
    {
        private record DebugMarkerPoint(Vector3 Position, float Radius, Color Color);
        private record DebugMarkerLine(Vector3 PositionA, Vector3 PositionB, float Thickness, Color Color);

        private static List<DebugMarkerPoint> s_markerPoints = new();
        private static List<DebugMarkerLine> s_markerLines = new();

        public static void Main(string[] args)
        {
            int i = 0;

            (int x, int y, int w, int h)? windowConfiguration = null;

            Camera2D cam = new Camera2D();

            Task.Run(() =>
            {
                while (true)
                {
                    var line = Console.ReadLine();

                    var parts = line!.Split(' ');
                    Console.WriteLine(parts);
                    if (parts.Length > 0)
                    {
                        switch(parts[0])
                        {
                            case "Win":
                                windowConfiguration = (
                                    int.Parse(parts[1]), 
                                    int.Parse(parts[2]), 
                                    int.Parse(parts[3]), 
                                    int.Parse(parts[4]));
                                break;
                            case "Cam":
                                cam = new Camera2D()
                                {
                                    offset = cam.offset,
                                    target = new Vector2(
                                    float.Parse(parts[1]),
                                    float.Parse(parts[2])
                                    ),
                                    zoom = float.Parse(parts[3])
                                };
                                break;
                            case "Clr":
                                s_markerPoints.Clear();
                                break;
                            case "Pnt":
                                s_markerPoints.Add(new DebugMarkerPoint(
                                    new Vector3(
                                        float.Parse(parts[1]),
                                        float.Parse(parts[2]),
                                        float.Parse(parts[3])
                                        ),
                                    float.Parse(parts[4])
,
                                    new Color(
                                        Convert.ToByte(parts[5][0..2], 16),
                                        Convert.ToByte(parts[5][2..4], 16),
                                        Convert.ToByte(parts[5][4..6], 16),
                                        Convert.ToByte(parts[5][6..8], 16)
                                        )));
                                break;
                            case "Lin":
                                s_markerLines.Add(new DebugMarkerLine(
                                    new Vector3(
                                        float.Parse(parts[1]),
                                        float.Parse(parts[2]),
                                        float.Parse(parts[3])
                                        ),
                                     new Vector3(
                                        float.Parse(parts[4]),
                                        float.Parse(parts[5]),
                                        float.Parse(parts[6])
                                        ),
                                    float.Parse(parts[7])
,
                                    new Color(
                                        Convert.ToByte(parts[8][0..2], 16),
                                        Convert.ToByte(parts[8][2..4], 16),
                                        Convert.ToByte(parts[8][4..6], 16),
                                        Convert.ToByte(parts[8][6..8], 16)
                                        )));
                                break;
                        }
                    }
                    
                }
            });

            bool isHidden = true;

            const int screenWidth = 1400;
            const int screenHeight = 900;

            ConfigFlags flags = ConfigFlags.FLAG_MSAA_4X_HINT;

            flags |= ConfigFlags.FLAG_WINDOW_UNDECORATED;
            flags |= ConfigFlags.FLAG_WINDOW_TRANSPARENT;
            flags |= ConfigFlags.FLAG_WINDOW_TOPMOST;
            flags |= ConfigFlags.FLAG_WINDOW_MOUSE_PASSTHROUGH;
            flags |= ConfigFlags.FLAG_WINDOW_UNFOCUSED;
            flags |= ConfigFlags.FLAG_WINDOW_ALWAYS_RUN;

            SetConfigFlags(flags);
            InitWindow(screenWidth, screenHeight, "CitySim");

            SetTargetFPS(15);

            IsoMetricGrid grid = new(128, 64, 32);
            
            while (!WindowShouldClose())
            {
                cam.offset = new Vector2(GetScreenWidth(), GetScreenHeight()) / 2;

                if (isHidden && windowConfiguration is not null)
                {
                    SetWindowState(flags);
                    isHidden = false;
                }

                //if (isHidden)
                //    return;

                if (windowConfiguration is not null)
                {
                    var (x, y, w, h) = windowConfiguration!.Value;

                    SetWindowPosition(x, y);
                    SetWindowSize(w, h);
                }


                BeginDrawing();
                ClearBackground(BLANK);

                BeginMode2D(cam);

                for (int j = 0; j < s_markerLines.Count; j++)
                {
                    var (posA, posB, thickness, color) = s_markerLines[j];

                    var posA2d = grid.GetPosition2D(posA);
                    var posB2d = grid.GetPosition2D(posB);

                    DrawLineEx(posA2d, posB2d, thickness, color);
                }

                for (int j = 0; j < s_markerPoints.Count; j++)
                {
                    var (pos, radius, color) = s_markerPoints[j];

                    var pos2d = grid.GetPosition2D(pos);

                    DrawCircleV(pos2d, radius, color);
                }
                

                EndMode2D();

                double a = Math.Sin(GetTime())*0.5+0.5;

                DrawRectangleLinesEx(new Rectangle(1, 1, GetScreenWidth() - 2, GetScreenHeight() - 2), 5, new Color(0, 255, (int)(255 * a), 255));
                EndDrawing();
                //i++;
            }
        }
    }
}
