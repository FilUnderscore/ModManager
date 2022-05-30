using UnityEngine;

namespace CustomModManager.UI
{
    public class XUiC_ModsErrorMessageBoxWindowGroup : XUiController
    {
        private string title = "";
        private string text = "";
        private string stackTraceText = "";
        private string copyText = "";
        public static string ID = "";
        public bool OpenMainMenuOnClose = false;
        
        public string LeftButtonText = "";
        public string RightButtonText = "";

        public string Title
        {
            get => title;
            set
            {
                this.title = value;
                this.IsDirty = true;
            }
        }

        public string Text
        {
            get => text;
            set
            {
                this.text = value;
                this.IsDirty = true;
            }
        }

        public event System.Action OnLeftButtonEvent;
        public event System.Action OnRightButtonEvent;

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch(_bindingName)
            {
                case "msgTitle":
                    _value = this.title;
                    return true;
                case "msgText":
                    _value = this.text;
                    return true;
                case "showleftbutton":
                    _value = (LeftButtonText.Length > 0).ToString();
                    return true;
                case "rightbuttontext":
                    _value = RightButtonText;
                    return true;
                case "leftbuttontext":
                    _value = LeftButtonText;
                    return true;
                case "stackTraceTooltip":
                    _value = stackTraceText;
                    return true;
                default:
                    return false;
            }
        }

        public override void Init()
        {
            base.Init();
            XUiC_ModsErrorMessageBoxWindowGroup.ID = this.WindowGroup.ID;
            XUiController childById1 = this.GetChildById("clickable2");
            if (childById1 != null)
                childById1.ViewComponent.Controller.OnPress += new XUiEvent_OnPressEventHandler(this.leftButton_OnPress);
            XUiController childById2 = this.GetChildById("clickable");
            if (childById2 != null)
                childById2.ViewComponent.Controller.OnPress += new XUiEvent_OnPressEventHandler(this.rightButton_OnPress);

            XUiController btnCopy = this.GetChildById("btnCopy");
            if (btnCopy == null)
                return;

            btnCopy.ViewComponent.Controller.OnPress += new XUiEvent_OnPressEventHandler(this.copyButton_OnPress);
        }

        private void copyButton_OnPress(XUiController _sender, int _mouseButton)
        {
            GUIUtility.systemCopyBuffer = this.copyText;
            GameManager.ShowTooltip(_sender.xui.playerUI.entityPlayer, Localization.Get("xuiGameModErrorCopied"));
        }

        private void leftButton_OnPress(XUiController _sender, int _mouseButton)
        {
            if (this.OnLeftButtonEvent != null)
                this.OnLeftButtonEvent();

            this.xui.playerUI.windowManager.Close(this.windowGroup.ID);
        }

        private void rightButton_OnPress(XUiController _sender, int _mouseButton)
        {
            if (this.OnRightButtonEvent != null)
                this.OnRightButtonEvent();

            this.xui.playerUI.windowManager.Close(this.windowGroup.ID);
        }

        public override void Update(float _dt)
        {
            base.Update(_dt);
            if (!this.IsDirty)
                return;

            this.RefreshBindings(true);
            this.IsDirty = false;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            this.windowGroup.isEscClosable = false;

            if(GameManager.Instance.World != null)
            {
                GameManager.Instance.Pause(true);
            }
        }

        public override void OnClose()
        {
            base.OnClose();

            if(GameManager.Instance.World != null)
            {
                GameManager.Instance.Pause(false);
            }

            if (!this.OpenMainMenuOnClose)
                return;
            this.xui.playerUI.windowManager.Open(XUiC_MainMenu.ID, true);
        }

        public void ShowMessage(string title, string text, string leftButtonText = "", string rightButtonText = "", System.Action onLeftButton = null, System.Action onRightButton = null, bool openMainMenuOnClose = false, bool closeAllWindows = false, string stackTraceText = "", string copyText = "")
        {
            this.Text = text;
            this.Title = title;
            this.LeftButtonText = leftButtonText;
            this.RightButtonText = rightButtonText;
            this.OnLeftButtonEvent = onLeftButton;
            this.OnRightButtonEvent = onRightButton;
            this.OpenMainMenuOnClose = openMainMenuOnClose;
            this.stackTraceText = stackTraceText;
            this.copyText = copyText;

            this.xui.playerUI.windowManager.Open(this.WindowGroup.ID, true, false, closeAllWindows);
        }

        public static void ShowMessageBox(XUi _xuiInstance, string title, string text, string leftButtonText = "", string rightButtonText = "", System.Action onLeftButton = null, System.Action onRightButton = null, bool openMainMenuOnClose = false, bool closeAllWindows = false, string stackTraceText = "", string copyText = "")
        {
            ((XUiC_ModsErrorMessageBoxWindowGroup)_xuiInstance.FindWindowGroupByName(XUiC_ModsErrorMessageBoxWindowGroup.ID)).ShowMessage(title, text, leftButtonText, rightButtonText, onLeftButton, onRightButton, openMainMenuOnClose, closeAllWindows, stackTraceText, copyText);
        }
    }
}