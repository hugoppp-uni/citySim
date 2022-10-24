using Raylib_CsLo;
using static CitySim.Frontend.RaylibExtensions;
using static Raylib_CsLo.Raylib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity;
using System.Reflection;
using CitySim.Backend.Entity.Structures;

namespace CitySim.Frontend
{
    public static class EnumExtensionMethods
    {
        public static string GetDescription(this Enum GenericEnum)
        {
            Type genericEnumType = GenericEnum.GetType();
            MemberInfo[] memberInfo = genericEnumType.GetMember(GenericEnum.ToString());
            if ((memberInfo != null && memberInfo.Length > 0))
            {
                var _Attribs = memberInfo[0].GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);
                if ((_Attribs != null && _Attribs.Count() > 0))
                {
                    return ((System.ComponentModel.DescriptionAttribute)_Attribs.ElementAt(0)).Description;
                }
            }
            return GenericEnum.ToString();
        }
    }

    internal class WorldDrawer
    {
        private enum Overlay
        {
            [System.ComponentModel.Description("Show Person Count")]
            PERSON_COUNT,
            [System.ComponentModel.Description("Show Grid Lines")]
            GRID_LINES,
            [System.ComponentModel.Description("Show Housing Score")]
            HOUSING_SCORE
        }

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

        private bool[] _overlaysEnabled;

        private List<string> _overlayNames;

        public IsoMetricGrid Grid { get; }

        public IReadOnlyList<string> OverlayNames => _overlayNames;

        public WorldDrawer(Backend.CitySim model)
        {
            _terrainSheet = SpriteSheet.FromPNG_XML(
            Path.Combine("Assets", "landscapeTiles_sheet.png"),
            Path.Combine("Assets", "landscapeTiles_sheet.xml"));

            _buildingSheet = SpriteSheet.FromPNG_XML(
            Path.Combine("Assets", "buildingTiles_sheet.png"),
            Path.Combine("Assets", "buildingTiles_sheet.xml"));

            Grid = new IsoMetricGrid(128, 64, 32);
            _model = model;

            _overlayNames = Enum.GetValues<Overlay>().Select(x => x.GetDescription()).ToList();
            _overlaysEnabled = new bool[_overlayNames.Count];
        }

        public void ToggleOverlay(int overlay, bool enabled)
        {
            _overlaysEnabled[overlay] = enabled;
        }

        public bool IsOverlayEnabled(int overlay)
        {
            return _overlaysEnabled[overlay];
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

            bool InCityBoundsX(int tileX) => tileX > 0 && tileX < _model!.WorldLayer.GridEnvironment.DimensionX;
            bool InCityBoundsY(int tileY) => tileY > 0 && tileY < _model!.WorldLayer.GridEnvironment.DimensionY;

            bool IsGround(int tileX, int tileY)
            {
                Vector2 pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

                return Vector2.Distance(pos, new Vector2(202, 40)) < 205;
            }

            var coordsWithPerson = _model.WorldLayer.GridEnvironment.Entities.OfType<Person>()
                .GroupBy(p => (p.Position.X, p.Position.Y))
                .ToDictionary(x=>x.Key, x=>x.Count());

            bool IsRoad(int tileX, int tileY) => _model.WorldLayer.Structures[tileX, tileY]?.GetType() == typeof(Street);

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

                    if (connections == 0)
                    {
                        DrawTerrainTile(67, position2d);
                        continue;
                    }

                    DrawTerrainTile(s_roadMap[connections], position2d);
                }
                else
                if (_model.WorldLayer.Structures[cell_x, cell_y]?.GetType() == typeof(House))
                {
                    DrawBuildingTile(1, position2d);

                    int height = (int)Math.Round(3 + Math.Sin(cell_x + 3) + Math.Sin(cell_y + 3));

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

                if (coordsWithPerson.TryGetValue((cell_x, cell_y), out int count))
                {
                    Vector2 pos = position2d - new Vector2(1, 1);

                    MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, new Color(0, 0, 0, 50));
                    DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, new Color(0, 0, 0, 50));

                    pos = position2d;

                    MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, WHITE);
                    DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, WHITE);
                }
            }

            if (_overlaysEnabled[(int)Overlay.GRID_LINES])
            {
                foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
                {
                    unsafe
                    {
                        var points = stackalloc Vector2[]
                        {
                            position2d,
                            position2d + new Vector2(0, -Grid.DiagSpanY / 2),
                            position2d + new Vector2(-Grid.DiagSpanX / 2, 0),
                            position2d + new Vector2(0, Grid.DiagSpanY / 2),
                        };

                        //DrawTriangleFan(points, 4, GOLD);

                        var linePoints = stackalloc Vector2[]
                        {
                            position2d + new Vector2(-Grid.DiagSpanX / 2, 0),
                            position2d + new Vector2(0, -Grid.DiagSpanY / 2),
                            position2d + new Vector2(-Grid.DiagSpanX / 2, 0),
                            position2d + new Vector2(0, Grid.DiagSpanY / 2),
                            position2d + new Vector2(-Grid.DiagSpanX / 2, 0),
                        };

                        DrawLineStrip(linePoints, 5, new Color(0, 0, 0, 20));
                    }
                }
            }

            if (_overlaysEnabled[(int)Overlay.PERSON_COUNT])
            {
                foreach (var (coord, count) in coordsWithPerson)
                {
                    Vector2 pos = Grid.GetPosition2D(new Vector3((float)coord.X, (float)coord.Y, 0));

                    string text = count.ToString();

                    int width = MeasureText(text, 20);
                    DrawEllipse((int)pos.X, (int)pos.Y - 0, 25, 15, ORANGE);
                    DrawText(text, pos.X - width / 2 - 1, pos.Y - 0 - 10, 20, new Color(0, 0, 0, 255));
                }
            }

            if (_overlaysEnabled[(int)Overlay.HOUSING_SCORE])
            {
                foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
                {
                    var d = _model.WorldLayer.BuildPositionEvaluator.HousingScore[cell_x, cell_y];
                    if (d is null || _model.WorldLayer.Structures[cell_x, cell_y] is not null)
                        continue;
                    var co = ColorFromHSV((float)d * 180, 1f, 1f);
                    co.a = 200;
                    
                    //todo proper draw
                    DrawRectangle((int)position2d.X, (int)position2d.Y, 15, 15, co);
                }
            }
        }
    }
}