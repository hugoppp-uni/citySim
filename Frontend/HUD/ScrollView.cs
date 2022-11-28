using Raylib_CsLo;
using System.Numerics;
using static Raylib_CsLo.Raylib;

namespace CitySim.Frontend
{
    internal class ScrollView : IScrollView
    {
        private (float width, float height) scrollBounds;
        private Vector2 scrollOffset;
        private Rectangle viewBounds;

        public Vector2 ScrollOffset
        {
            get => scrollOffset; set
            {
                scrollOffset = value;
                EnforceValidScrollOffset();
            }
        }
        public (float width, float height) ScrollBounds
        {
            get => scrollBounds;
            set
            {
                if (ScrollBounds.width < 0)
                    throw new ArgumentException("width cannot be negative");

                scrollBounds = value;
                EnforceValidScrollOffset();
            }
        }

        public Rectangle ViewBounds
        {
            get => viewBounds;
            set
            {
                viewBounds = value;
                EnforceValidScrollOffset();
            }
        }

        private void EnforceValidScrollOffset()
        {
            if (scrollBounds.width <= ViewBounds.width)
                scrollOffset.X = 0;
            else
            {
                scrollOffset.X = Math.Clamp(scrollOffset.X, 0, scrollBounds.width - ViewBounds.width);
            }

            if (scrollBounds.height <= ViewBounds.height)
                scrollOffset.Y = 0;
            else
            {
                scrollOffset.Y = Math.Clamp(scrollOffset.Y, 0, scrollBounds.height - ViewBounds.height);
            }
        }

        public ScrollView((float width, float height) scrollBounds, Rectangle viewBounds)
        {
            ScrollBounds = scrollBounds;
            ViewBounds = viewBounds;
        }

        public void Update(bool isHovered)
        {
            if (isHovered)
                ScrollOffset -= GetMouseWheelMoveV() * 20;
        }
    }

    internal interface IScrollView
    {
        (float width, float height) ScrollBounds { get; set; }
        Vector2 ScrollOffset { get; set; }
        Rectangle ViewBounds { get; set; }
    }
}
