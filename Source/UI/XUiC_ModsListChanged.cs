using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomModManager.UI
{
    public class XUiC_ModsListChanged : XUiController
    {
        public static string ID = "";

        public override void Init()
        {
            base.Init();

            ID = this.WindowGroup.ID;
            ((XUiC_SimpleButton)this.GetChildById("btnOk")).OnPressed += BtnOk_OnPressed;
        }

        private void BtnOk_OnPressed(XUiController _sender, int _mouseButton)
        {
            this.xui.playerUI.windowManager.Close(this.windowGroup.ID);
            this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.windowGroup.openWindowOnEsc = XUiC_MainMenu.ID;
        }
    }
}
