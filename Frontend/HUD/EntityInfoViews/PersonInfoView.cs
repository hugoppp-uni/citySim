using CitySim.Backend.Entity.Agents;
using CitySim.Backend.Entity.Structures;
using Mars.Interfaces.Agents;
using Raylib_CsLo;

using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal class PersonInfoView : ScrollView, IEntityInfoView
    {
        public PersonInfoView(Person person, (float width, float height) scrollBounds, Rectangle viewBounds)
            : base(scrollBounds, viewBounds)
        {
            Person = person;
        }

        public Person Person { get; }

        public IEntity Entity => Person;

        public void UpdateAndDraw(bool isHovered)
        {
            Update(isHovered);

            string infoText = @"
Lorem ipsum dolor sit amet consectetur adipisicing elit. Maxime mollitia,
molestiae quas vel sint commodi repudiandae consequuntur voluptatum laborum
numquam blanditiis harum quisquam eius sed odit fugiat iusto fuga praesentium
optio, eaque rerum! Provident similique accusantium nemo autem. Veritatis
obcaecati tenetur iure eius earum ut molestias architecto voluptate aliquam
nihil, eveniet aliquid culpa officia aut! Impedit sit sunt quaerat, odit,
tenetur error, harum nesciunt ipsum debitis quas aliquid.".Replace('\r', ' ');

            {
                //Draw (and layout)

                var b = ViewBounds;
                BeginScissorMode((int)b.X, (int)b.Y, (int)b.width, (int)b.height);

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


                Text(font, 30, x, "Info about Person\n", WHITE);

                Text(font, 20, x, $"Current action: {Person.GetNextAction()}", GREEN);
                Text(font, 20, x, $"Hunger: {Math.Round(Person.Needs.Hunger, 2)}", GREEN);
                Text(font, 20, x, $"Sleepiness: {Math.Round(Person.Needs.Sleepiness, 2)}", GREEN);

                Text(font, 20, x, infoText, new Color(200, 200, 200, 255));


                EndScissorMode();


                ScrollBounds = (maxWidth, y - startY);
            }
        }
    }
}
