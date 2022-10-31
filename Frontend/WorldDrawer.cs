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
using static ServiceStack.Script.Lisp;

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

        private const int TILE_BUILDING_FLOOR = 0;
        private const int TILE_BUILDING_GROUND = 1;
        private const int TILE_RESTAURANT_BOTTOMLEFT = 4;
        private const int TILE_BUILDING_ROOF = 5;
        private const int TILE_GRASS_GROUND = 67;

        private readonly Backend.CitySim _model;
        private readonly SpriteSheet _terrainSheet;
        private readonly SpriteSheet _buildingSheet;
        private static readonly Dictionary<byte, int> s_roadMap = new Dictionary<byte, int>()
        {
            //connections bitfield: topLeft, topRight, bottomRight, bottomLeft

            //<0>     <1>
            //    < >
            //<3>     <2>

            //b_0123, TILE_ID
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

        private static bool TryGetRoadConnectionTile(bool topLeft, bool topRight, bool bottomRight, bool bottomLeft, out int tile)
        {
            byte bitField = 0;

            if (topLeft)
                bitField |= 0b1000;
            if (topRight)
                bitField |= 0b0100;
            if (bottomRight)
                bitField |= 0b0010;
            if (bottomLeft)
                bitField |= 0b0001;

            return s_roadMap.TryGetValue(bitField, out tile);
        }


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

        private static Vector3 GetPersonPosition(Person person)
        {
            int hash = person.GetHashCode();
            float xoff = (uint)hash % 1000 / 1000f - 0.5f;
            float yoff = (uint)(hash * 15485863) % 1000 / 1000f - 0.5f;

            return new((float)person.Position.X + xoff, (float)person.Position.Y + yoff, 0);
        }

        private void DrawBuildingTile(int tile, Vector2 bottomCenter)
        {
            string name = $"buildingTiles_{tile:000}.png";
            var rect = _buildingSheet.Rects[name];
            _buildingSheet.DrawSprite(name,
                bottomCenter - new Vector2(rect.width / 2, rect.height - 64));
        }

        private void DrawTerrainTile(int tile, Vector2 bottomCenter)
        {
            string name = $"landscapeTiles_{tile:000}.png";
            var rect = _terrainSheet.Rects[name];
            _terrainSheet.DrawSprite(name,
                bottomCenter - new Vector2(rect.width / 2, rect.height - 64));
        }

        private static bool IsGround(int tileX, int tileY)
        {
            Vector2 pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

            return Vector2.Distance(pos, new Vector2(202, 40)) < 205;
        }

        private bool IsRoad(int tileX, int tileY) => _model.WorldLayer.Structures[tileX, tileY] is Street;

        private static void DrawPerson(Person person, Vector2 position2d)
        {
            Vector2 pos = position2d - new Vector2(1, 1);

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, new Color(0, 0, 0, 50));
            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, new Color(0, 0, 0, 50));

            pos = position2d;

            int hash = person.GetHashCode();
            Color col = ColorFromHSV((uint)hash % 360, 70, 100);

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, col);
            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, WHITE);
        }

        private void DrawBuilding(Vector2 position2d, float cell_height, int stories)
        {
            DrawBuildingTile(TILE_BUILDING_GROUND, position2d);

            for (int i = 0; i < stories; i++)
            {
                DrawBuildingTile(TILE_BUILDING_FLOOR, position2d - new Vector2(0,
                    cell_height * (2.3f + i)));
            }

            DrawBuildingTile(TILE_BUILDING_ROOF, position2d - new Vector2(0,
                    cell_height * (2.3f + stories)));
        }

        public void Draw(Camera2D camera)
        {

            var personsGroupedByCoord = _model.WorldLayer.GridEnvironment.Entities.OfType<Person>()
                .GroupBy(p => (p.Position.X, p.Position.Y))
                .ToDictionary(x => x.Key, x => x);

            foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
            {
                if (!IsGround(cell_x, cell_y))
                    continue;

                

                if (IsRoad(cell_x, cell_y))
                {
                    if(TryGetRoadConnectionTile(
                        IsRoad(cell_x, cell_y + 1),
                        IsRoad(cell_x + 1, cell_y),
                        IsRoad(cell_x, cell_y - 1),
                        IsRoad(cell_x - 1, cell_y), 
                        out int tile))
                    {
                        DrawTerrainTile(tile, position2d);
                    }
                    else
                    {
                        DrawTerrainTile(67, position2d);
                    }
                }
                else if (_model.WorldLayer.Structures[cell_x, cell_y]?.GetType() == typeof(Restaurant))
                {
                    DrawBuildingTile(TILE_RESTAURANT_BOTTOMLEFT, position2d);
                }
                else if (_model.WorldLayer.Structures[cell_x, cell_y]?.GetType() == typeof(House))
                {
                    int stories = (int)Math.Round(3 + Math.Sin(cell_x + 3) + Math.Sin(cell_y + 3));
                    DrawBuilding(position2d, cell_height, stories);
                }
                else
                {
                    DrawTerrainTile(TILE_GRASS_GROUND, position2d);
                }


                //Draw persons
                if (personsGroupedByCoord.TryGetValue((cell_x, cell_y), out IGrouping<(double, double), Person>? persons))
                {
                    foreach (var person in persons)
                    {
                        Vector2 _position2d = Grid.GetPosition2D(GetPersonPosition(person));
                        DrawPerson(person, _position2d);
                    }

                }
            }

            if (_overlaysEnabled[(int)Overlay.GRID_LINES])
            {
                foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
                {
                    unsafe
                    {
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
                foreach (var (coord, persons) in personsGroupedByCoord)
                {
                    Vector2 pos = Grid.GetPosition2D(new Vector3((float)coord.X, (float)coord.Y, 0));

                    string text = persons.Count().ToString();

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

                    var val = (float)d.Value;


                    var co = ColorFromHSV(val * 180, 1f, 1f);
                    co.a = (byte)(100 * val);

                    unsafe
                    {
                        var up = new Vector2(0, -Grid.DiagSpanY / 2);
                        var down = new Vector2(0, Grid.DiagSpanY / 2);
                        var left = new Vector2(-Grid.DiagSpanX / 2, 0);
                        var right = new Vector2(Grid.DiagSpanX / 2, 0);

                        var points = stackalloc Vector2[]
                        {
                            position2d + right * val,
                            position2d + up * val,
                            position2d + down * val,
                            position2d + left * val,
                        };

                        DrawTriangleStrip(points, 4, co);
                    }
                }
            }
        }
    }
}