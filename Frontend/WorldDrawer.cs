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
        private record SplitAnim(Person PersonA, Person PersonB, double? StartTime);

        private enum Overlay
        {
            [System.ComponentModel.Description("Show Person Count")]
            PERSON_COUNT,
            [System.ComponentModel.Description("Show Grid Lines")]
            GRID_LINES,
            [System.ComponentModel.Description("Show Housing Score")]
            HOUSING_SCORE,
        }

        private const int TILE_BUILDING_FLOOR = 0;
        private const int TILE_BUILDING_GROUND = 1;
        private const int TILE_RESTAURANT_BOTTOMLEFT = 4;
        private const int TILE_BUILDING_ROOF = 5;
        private const int TILE_GRASS_GROUND = 67;

        private readonly List<SplitAnim> _splitAnimations = new();
        private readonly HashSet<Person> _splittingPersons = new HashSet<Person>();

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
        private const int Stories = 1;

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

        public IPositionableEntity? HoveredEntity { get; private set; }

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

            List<Person> people = model.WorldLayer.GridEnvironment.Entities.OfType<Person>().ToList();

            model.WorldLayer.PersonCellDivision += (a, b) =>
            {
                lock(_splitAnimations)
                    _splitAnimations.Add(new(a, b, null));
            };
        }

        public void ToggleOverlay(int overlay, bool enabled)
        {
            _overlaysEnabled[overlay] = enabled;
        }

        public bool IsOverlayEnabled(int overlay)
        {
            return _overlaysEnabled[overlay];
        }

        public Vector2 GetPersonPosition2D(Person person)
        {
            return Grid.GetPosition2D(GetPersonPosition(person));
        }

        public Vector2 GetEntityPosition2D(IPositionableEntity entity)
        {
            if (entity is Person person)
                return GetPersonPosition2D(person);

            return Grid.GetPosition2D(new Vector3(
                (float)entity.Position.X, 
                (float)entity.Position.Y, 0));
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
            var pos = new Vector2(tileX + 0.5f, tileY + 0.5f);

            return Vector2.Distance(pos, new Vector2(202, 40)) < 206;
        }

        private bool IsRoad(int tileX, int tileY) => _model.WorldLayer.Structures[tileX, tileY] is Street;



        public static float GetPersonVisualHeight(Camera2D cam) => 50 * cam.zoom;
        private static void DrawPerson(Person person, Vector2 position2d, Color? highlight)
        {
            static Color Tint(Color col, Color highlight)
            {
                col = new Color(
                    (int)(255 * (col.r / 255f * 0.7f + highlight.r / 255f * 0.3f)),
                    (int)(255 * (col.g / 255f * 0.7f + highlight.g / 255f * 0.3f)),
                    (int)(255 * (col.b / 255f * 0.7f + highlight.b / 255f * 0.3f)),
                    col.a);
                return col;
            }


            Vector2 pos = position2d - new Vector2(1, 1);

            //shadow
            Color col = highlight ?? new Color(0, 0, 0, 50);

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, col);
            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, col);

            pos = position2d;


            //color
            col = GetPersonColor(person);

            if (highlight is not null)
                col = Tint(col, highlight.Value);

            MyDrawRoundedRect(pos.X - 10, pos.Y - 25, pos.X + 10, pos.Y, 5, col);

            col = WHITE;

            if (highlight is not null)
                col = Tint(col, highlight.Value);

            DrawEllipse((int)pos.X, (int)pos.Y - 40, 10, 10, col);
        }

        public static Color GetPersonColor(Person person)
        {
            Color col;
            int hash = person.GetHashCode();
            col = ColorFromHSV((uint)hash % 360, 70, 100);
            return col;
        }

        private static bool HitTestPerson(Vector2 position2d, Vector2 hitPoint)
        {
            Vector2 pos = position2d;

            bool isHitHead = Vector2.Distance(pos + new Vector2(0, -40), hitPoint) < 10;

            bool isHitBody = Vector2.Distance(new Vector2(
                Math.Clamp(hitPoint.X, pos.X - 5, pos.X + 5),
                Math.Clamp(hitPoint.Y, pos.Y - 20, pos.Y - 5)
                ), hitPoint) < 5;

            return isHitHead || isHitBody;
        }

        public static float GetHouseBlockHeight() => Stories+1.1f;

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

        private void DrawBuildingBoundingBox(Vector2 position2d, float cell_height, int stories)
        {
            unsafe
            {
                var buildingTopCenter = position2d - new Vector2(0, cell_height * (stories+1));

                var up = new Vector2(0, -Grid.DiagSpanY / 2);
                var down = new Vector2(0, Grid.DiagSpanY / 2);
                var left = new Vector2(-Grid.DiagSpanX / 2, 0);
                var right = new Vector2(Grid.DiagSpanX / 2, 0);

                {
                    var points = stackalloc Vector2[]
                    {
                        buildingTopCenter + right,
                        buildingTopCenter + down,
                        buildingTopCenter + left,
                        buildingTopCenter + up,
                        buildingTopCenter + right,
                    };

                    DrawLineStrip(points, 5, WHITE);
                }

                {
                    var points = stackalloc Vector2[]
                    {
                        buildingTopCenter + down,
                        buildingTopCenter + right,

                        position2d + right,
                        position2d + down,

                        buildingTopCenter + down,
                    };

                    DrawLineStrip(points, 5, WHITE);
                }

                {
                    var points = stackalloc Vector2[]
                    {
                        buildingTopCenter + left,
                        buildingTopCenter + down,

                        position2d + down,
                        position2d + left,
                        buildingTopCenter + left,
                    };

                    DrawLineStrip(points, 5, WHITE);
                }
            }
        }

        private bool HitTestBuildingBoundingBox(Vector2 position2d, float cell_height, int stories, Vector2 hitPoint)
        {
            bool hovered = false;

            var buildingTopCenter = position2d - new Vector2(0, cell_height * (stories + 1));

            var up = new Vector2(0, -Grid.DiagSpanY / 2);
            var down = new Vector2(0, Grid.DiagSpanY / 2);
            var left = new Vector2(-Grid.DiagSpanX / 2, 0);
            var right = new Vector2(Grid.DiagSpanX / 2, 0);

            hovered |= Util.IsPointInQuad(hitPoint,
                buildingTopCenter + right,
                buildingTopCenter + down,
                buildingTopCenter + left,
                buildingTopCenter + up);
                
            hovered |= Util.IsPointInQuad(hitPoint,
                buildingTopCenter + down,
                buildingTopCenter + right,

                position2d + right,
                position2d + down);

            hovered |= Util.IsPointInQuad(hitPoint,
                buildingTopCenter + left,
                buildingTopCenter + down,

                position2d + down,
                position2d + left);

            return hovered;
        }

        public void Draw(Camera2D camera, bool isHovered)
        {
            double time = GetTime();

            var hitPoint = GetScreenToWorld2D(GetMousePosition(), camera);

            lock (_splitAnimations)
            {
                for (int i = _splitAnimations.Count - 1; i >= 0; i--)
                {
                    var anim = _splitAnimations[i];
                    if (anim.StartTime + 1.6f < GetTime())
                    {
                        _splittingPersons.Remove(anim.PersonA);
                        _splittingPersons.Remove(anim.PersonB);
                        _splitAnimations.RemoveAt(i);
                        continue;
                    }

                    if (anim.StartTime is null &&
                        !_splittingPersons.Contains(anim.PersonA) &&
                        !_splittingPersons.Contains(anim.PersonB))
                    {
                        _splitAnimations[i] = _splitAnimations[i] with { StartTime = GetTime() };
                        _splittingPersons.Add(anim.PersonA);
                        _splittingPersons.Add(anim.PersonB);
                    }
                }
            }

            IPositionableEntity? newHoveredEntity = null;

            var personsGroupedByCoord = _model.WorldLayer.GridEnvironment.Entities.OfType<Person>()
                .GroupBy(p => (p.Position.X, p.Position.Y))
                .ToDictionary(x => x.Key, x => x);

            foreach (var (cell_x, cell_y, position2d, cell_height) in Grid.GetVisibleCells(camera))
            {
                if (!IsGround(cell_x, cell_y))
                    continue;



                if (IsRoad(cell_x, cell_y))
                {
                    if (TryGetRoadConnectionTile(
                        IsRoad(cell_x - 1, cell_y),
                        IsRoad(cell_x, cell_y + 1),
                        IsRoad(cell_x + 1, cell_y),
                        IsRoad(cell_x, cell_y - 1),
                        out int tile))
                    {
                        DrawTerrainTile(tile, position2d);
                    }
                    else
                    {
                        DrawTerrainTile(67, position2d);
                    }
                }
                else if (_model.WorldLayer.Structures[cell_x, cell_y] is Restaurant)
                {
                    DrawBuildingTile(TILE_RESTAURANT_BOTTOMLEFT, position2d);
                }
                else if (_model.WorldLayer.Structures[cell_x, cell_y] is House house)
                {
                    DrawBuilding(position2d, cell_height, Stories);

                    if(HoveredEntity == house)
                        DrawBuildingBoundingBox(position2d, cell_height, Stories);

                    if (HitTestBuildingBoundingBox(position2d, cell_height, Stories, hitPoint))
                        newHoveredEntity = house;
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
                        if(_splittingPersons.Contains(person))
                            continue;

                        Vector2 _position2d = Grid.GetPosition2D(GetPersonPosition(person));
                        DrawPerson(person, _position2d, HoveredEntity == person ? BLUE : null);

                        if (HitTestPerson(_position2d, hitPoint))
                            newHoveredEntity = person;
                    }
                }
            }

            //Draw splitting persons
            lock (_splitAnimations)
            {
                foreach (var (personA, personB, startTime) in _splitAnimations)
                {
                    if (startTime is null)
                        continue;

                    var posA = GetPersonPosition2D(personA);
                    var posB = GetPersonPosition2D(personB);

                    var colA = GetPersonColor(personA);
                    var colB = GetPersonColor(personB);

                    SplittingPersonDrawer.Draw((float)(time - startTime!) / 1.6f,
                        posA, posA, WHITE, colA,
                        posA, posB, WHITE, colB);
                }
            }

            DrawOverlays(camera, personsGroupedByCoord);

            if (!isHovered)
                newHoveredEntity = null;

            if (newHoveredEntity is null)
                SetMouseCursor(MouseCursor.MOUSE_CURSOR_ARROW);
            else
                SetMouseCursor(MouseCursor.MOUSE_CURSOR_POINTING_HAND);

            HoveredEntity = newHoveredEntity;
        }

        private void DrawOverlays(Camera2D camera, Dictionary<(double X, double Y), IGrouping<(double X, double Y), Person>> personsGroupedByCoord)
        {
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
                    if (d is null or double.NegativeInfinity)
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