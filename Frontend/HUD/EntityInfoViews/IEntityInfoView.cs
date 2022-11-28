using Mars.Interfaces.Agents;

namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal interface IEntityInfoView : IScrollView
    {
        public IEntity Entity { get; }

        void UpdateAndDraw(bool isHovered);
    }
}