namespace CustomModManager.UI.Wrappers
{
    public sealed class XUiW_Label : XUiW<XUiV_Label>
    {
        private readonly Vector2i originalPosition;

        public XUiW_Label(XUiController controller, string childName) : base(controller, childName)
        {
            this.originalPosition = this.ViewComponent.Position;
        }

        public void Offset(int offsetX, int offsetY)
        {
            this.ViewComponent.Position = this.originalPosition + new Vector2i(offsetX, offsetY);
        }
    }
}
