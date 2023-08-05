namespace CustomModManager.UI.Wrappers
{
    public abstract class XUiW<T> where T : XUiView
    {
        protected T ViewComponent;

        public XUiW(T viewComponent) 
        {
            this.ViewComponent = viewComponent;
        }

        public XUiW(XUiController controller, string childName)
        {
            this.ViewComponent = (T)controller.GetChildById(childName).ViewComponent;
        }
    }
}
