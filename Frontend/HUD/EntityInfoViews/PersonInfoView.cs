using System.Text;
using CitySim.Backend.Entity;
using CitySim.Backend.Entity.Agents;
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

        private readonly Person.PersonActionLog[] _actionLog = new Person.PersonActionLog[20];
        public Person Person { get; }

        public IPositionableEntity Entity => Person;

        void IEntityInfoView.HandleClick()
        {

        }

        public void UpdateAndDraw(bool isHovered)
        {
            Update(isHovered);
            int logLength = Person.GetActionLog(_actionLog);
            StringBuilder builder = new StringBuilder();
            for (var i = 0; i < logLength; i++)
            {
                builder.Append("Decided to ");
                builder.Append(_actionLog[i].Action.Type.ToString());
                builder.Append(" with the stats: ");
                builder.Append(_actionLog[i].Needs);
                builder.Append('\n');
            }

            var actionLog = builder.ToString();
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
                    
                    y += size.Y;
                }

                Font font = GetFontDefault();


                Text(font, 30, x, $"Info about {Person.Name}\n", WHITE);

                Text(font, 20, x, $"Current action: {Person.GetNextAction()}", GREEN);
                Text(font, 20, x, $"Hunger: {Math.Round(Person.Needs.Hunger, 2)}", GREEN);
                Text(font, 20, x, $"Money: {Person.Needs.Money}", GREEN);
                Text(font, 20, x, $"Sleepiness: {Math.Round(Person.Needs.Sleepiness, 2)}", GREEN);

                Text(font, 14, x, actionLog, new Color(200,200,200,255));


                EndScissorMode();


                ScrollBounds = (maxWidth, y - startY);
            }
        }
    }
}
