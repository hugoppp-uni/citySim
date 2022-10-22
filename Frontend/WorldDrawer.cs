using Raylib_CsLo;
using static CitySim.Frontend.RaylibExtensions;
using static Raylib_CsLo.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CitySim.Frontend
{
    internal class WorldDrawer
    {
        private readonly Backend.CitySim _model;
        private readonly SpriteSheet _terrainSheet;
        private readonly SpriteSheet _buildingSheet;
        private static readonly Dictionary<byte, int> s_roadMap = new Dictionary<byte, int>()
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

        public IsoMetricGrid Grid { get; }

        public WorldDrawer(CitySim.Backend.CitySim model)
        {
            _terrainSheet = SpriteSheet.FromPNG_XML(
            Path.Combine("Assets", "landscapeTiles_sheet.png"),
            Path.Combine("Assets", "landscapeTiles_sheet.xml"));

            _buildingSheet = SpriteSheet.FromPNG_XML(
            Path.Combine("Assets", "buildingTiles_sheet.png"),
            Path.Combine("Assets", "buildingTiles_sheet.xml"));

            Grid = new IsoMetricGrid(128, 64, 32);
            _model = model;
        }

        public void Draw(Camera2D camera)
        {
            void DrawTerrainTile(int tile, Vector2 bottomCenter)
            {
                string name = $"landscapeTiles_{tile:000}.png";
                var rect = _terrainSheet.Rects[name];
                _terrainSheet.DrawSprite(name,
                    bottomCenter - new Vector2(rect.width / 2, rect.height - 64));
            }

            void DrawBuildingTile(int tile, Vector2 bottomCenter)
            {
                string name = $"buildingTiles_{tile:000}.png";
                var rect = _buildingSheet.Rects[name];
                _buildingSheet.DrawSprite(name,
                    bottomCenter - new Vector2(rect.width / 2, rect.height - 64));
            }



            bool InCityBounds(int tileX, int tileY) =>
                InCityBoundsX(tileX) &&
                InCityBoundsY(tileY);

            bool InCityBoundsX(int tileX) => tileX > 0 && tileX < _model!.GridLayer.GridEnvironment.DimensionX;
            bool InCityBoundsY(int tileY) => tileY > 0 && tileY < _model!.GridLayer.GridEnvironment.DimensionY;

            bool IsGround(int tileX, int tileY)
            {
                Vector2 pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

                return Vector2.Distance(pos, new Vector2(202, 40)) < 205;
            }

            bool IsRoad(int tileX, int tileY) => (
                (tileX % 4 == 0 && InCityBoundsX(tileX)) ||
                (tileY % 3 == 0 && InCityBoundsY(tileY))
                ) && IsGround(tileX, tileY);


            foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
            {
                if (!IsGround(cell_x, cell_y))
                    continue;

                //connections bitfield: Y, X, -Y, -X

                byte connections = 0;

                if (IsRoad(cell_x, cell_y))
                {
                    if (IsRoad(cell_x, cell_y + 1))
                        connections |= 0b1000;
                    if (IsRoad(cell_x + 1, cell_y))
                        connections |= 0b0100;
                    if (IsRoad(cell_x, cell_y - 1))
                        connections |= 0b0010;
                    if (IsRoad(cell_x - 1, cell_y))
                        connections |= 0b0001;

                    DrawTerrainTile(s_roadMap[connections], position2d);
                }
                else if(InCityBounds(cell_x, cell_y))
                {
                    DrawBuildingTile(1, position2d);

                    int height = (int)Math.Round(3 + Math.Sin(cell_x+3)+ Math.Sin(cell_y + 3));

                    for (int i = 0; i < height; i++)
                    {
                        DrawBuildingTile(0, position2d - new Vector2(0, 
                            cell_height * (2.3f + i)));
                    }

                    DrawBuildingTile(5, position2d - new Vector2(0,
                            cell_height * (2.3f + height)));
                }
                else
                {
                    DrawTerrainTile(67, position2d);
                }

                if (_model.GridLayer.GridEnvironment.Explore(cell_x, cell_y, 0).Any())
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
        }
    }
}
