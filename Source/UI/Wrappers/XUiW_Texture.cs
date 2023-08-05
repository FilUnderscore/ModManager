namespace CustomModManager.UI.Wrappers
{
    public sealed class XUiW_Texture : XUiW<XUiV_Texture>
    {
        public XUiW_Texture(XUiController controller, string childName) : base(controller, childName)
        {
        }

        public void SetTexture(string modFolderRelativePath)
        {
            this.ViewComponent.IsVisible = false;
            this.ViewComponent.UnloadTexture();
            this.ViewComponent.ParseAttribute("texture", modFolderRelativePath, null);
            this.ViewComponent.UpdateData();
            this.ViewComponent.IsVisible = true;
        }

        public int GetHeight()
        {
            return this.ViewComponent.Size.y;
        }

        public int GetWidth()
        {
            return this.ViewComponent.Size.x;
        }
    }
}
