using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using Raylib_CsLo;

using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal class HouseInfoView : ScrollView, IEntityInfoView
    {
        public HouseInfoView(House house, (float width, float height) scrollBounds, Rectangle viewBounds)
            : base(scrollBounds, viewBounds)
        {
            House = house;
        }

        public House House { get; }

        public void UpdateAndDraw(bool isHovered)
        {
            Update(isHovered);

            {
                //Draw (and layout)

                var b = ViewBounds;
                //BeginScissorMode((int)b.X, (int)b.Y, (int)b.width, (int)b.height);

                float maxWidth = 0;

                var startX = ViewBounds.X - ScrollOffset.X;
                var x = startX;
                var startY = ViewBounds.Y - ScrollOffset.Y;
                var y = startY;

                void Text(Font font, float fontSize, float x, string text, Color color)
                {
                    DrawTextEx(font, text, new(x, y), fontSize, fontSize / font.baseSize, color);
                    var size = MeasureTextEx(font, text, fontSize, fontSize / font.baseSize);
                    maxWidth = Math.Max(maxWidth, x + size.X - startX);

                    y += size.Y;
                }

                Font font = GetFontDefault();


                Text(font, 30, x, "Info about House\n", WHITE);

                Text(font, 25, x, $"Inhabitants", LIGHTGRAY);


                for (int i = 0; i < House.MaxSpaces; i++)
                {
                    DrawRectangleRec(new(x, y, 20, 40),
                        new Color(50, 50, 50, 150));

                    if (i < House.Inhabitants.Count)
                    {
                        var person = House.Inhabitants[i];
                        var color = WorldDrawer.GetPersonColor(person);
                        DrawRectangleRec(new(x, y, 20, 20), WHITE);
                        DrawRectangleRec(new(x, y + 20, 20, 20), color);
                    }
                    x += 30;
                }

                y += 50;

                EndScissorMode();


                ScrollBounds = (maxWidth, y - startY);
            }
        }
    }
}
