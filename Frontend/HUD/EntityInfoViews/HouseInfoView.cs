using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal class HouseInfoView : ScrollView, IEntityInfoView
    {
        private Person? hoveredPerson;
        private Person? selectedPerson;

        private PersonInfoView? selectedPersonInfoView;

        public HouseInfoView(House house, (float width, float height) scrollBounds, Rectangle viewBounds)
            : base(scrollBounds, viewBounds)
        {
            House = house;
        }

        public House House { get; }

        public IPositionableEntity Entity => House;

        void IEntityInfoView.HandleClick()
        {
            selectedPerson = hoveredPerson;

            if (selectedPerson is not null)
                selectedPersonInfoView = new PersonInfoView(selectedPerson, (0, 0), ViewBounds);
            else
                selectedPersonInfoView = null;

        }

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

                Vector2 mousePos = GetMousePosition();



                Text(font, 30, x, "Info about House\n", WHITE);

                Text(font, 25, x, $"Inhabitants", LIGHTGRAY);

                hoveredPerson = null;


                for (int i = 0; i < House.MaxSpaces; i++)
                {
                    DrawRectangleRec(new(x, y, 20, 40),
                        new Color(50, 50, 50, 150));

                    var hoverRect = new Rectangle(
                        x - 5,
                        y,
                        30,
                        40
                        );

                    var personIsHovered = CheckCollisionPointRec(mousePos, hoverRect);

                    if (i < House.Inhabitants.Count)
                    {
                        var person = House.Inhabitants[i];

                        if (personIsHovered && isHovered)
                        {
                            DrawRectangleRec(hoverRect, new Color(0, 0, 0, 50));
                            hoveredPerson = person;
                        }

                        var color = WorldDrawer.GetPersonColor(person);
                        DrawRectangleRec(new(x, y, 20, 20), WHITE);
                        DrawRectangleRec(new(x, y + 20, 20, 20), color);
                    }
                    x += 30;
                }

                y += 50;

                EndScissorMode();

                if (selectedPersonInfoView is not null)
                {
                    selectedPersonInfoView.ViewBounds = new Rectangle(
                        ViewBounds.X + ViewBounds.width - 600,
                        ViewBounds.Y,
                        600,
                        ViewBounds.height
                        );

                    selectedPersonInfoView.UpdateAndDraw(isHovered);
                }


                ScrollBounds = (maxWidth, y - startY);
            }
        }
    }
}
