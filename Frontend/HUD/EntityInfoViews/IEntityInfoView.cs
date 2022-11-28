namespace CitySim.Frontend.HUD.EntityInfoViews
{
    internal interface IEntityInfoView : IScrollView
    {
        void UpdateAndDraw(bool isHovered);
    }
}