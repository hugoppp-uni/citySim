using CitySim.Backend.Entity;

namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal interface IEntityInfoView : IScrollView
    {
        public IPositionableEntity Entity { get; }

        void UpdateAndDraw(bool isHovered);
    }
}