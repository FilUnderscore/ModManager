﻿using UnityEngine;
using HarmonyLib;
using System.Reflection;

namespace CustomModManager.UI
{
    public class XUiC_ModSettingSelector : XUiController
    {
        private static readonly FieldInfo WrapField = AccessTools.Field(typeof(XUiC_ComboBox<ModOptionValue>), "Wrap");

        private XUiC_ComboBoxList<ModOptionValue> controlCombo;
        private XUiC_TextInput controlText;

        private XUiV_Label textLabel;
        private XUiV_Label label;

        private XUiC_SimpleButton button;

        public ModManagerModSettings.BaseModSetting modSetting;

        public XUiC_ModSettingSelector() { }

        public override void Init()
        {
            base.Init();

            this.controlCombo = this.GetChildById("ControlCombo").GetChildByType<XUiC_ComboBoxList<ModOptionValue>>();
            this.controlCombo.OnValueChanged += ControlCombo_OnValueChanged;
            this.controlText = this.GetChildById("ControlText").GetChildByType<XUiC_TextInput>();
            this.controlText.OnChangeHandler += ControlText_OnChangeHandler;
            this.controlText.OnSelect += ControlText_OnSelect;
            this.textLabel = this.GetChildById("ControlLabel").ViewComponent as XUiV_Label;
            this.label = this.GetChildById("ControlLabel2").ViewComponent as XUiV_Label;
            this.button = this.GetChildById("ControlButton") as XUiC_SimpleButton;
            this.button.OnPressed += Button_OnPressed;
        }

        private void Button_OnPressed(XUiController _sender, int _mouseButton)
        {
            if (this.modSetting == null || !(this.modSetting is ModManagerModSettings.ButtonModSetting))
            {
                return;
            }

            (this.modSetting as ModManagerModSettings.ButtonModSetting).GetClickCallback().Invoke();
        }

        public void UpdateModSetting(ModManagerModSettings.BaseModSetting modSetting)
        {
            if(this.modSetting != null)
                this.modSetting.selector = null;
    
            this.modSetting = modSetting;
            modSetting.selector = this;

            if (this.modSetting != null)
            {
                if(this.modSetting is ModManagerModSettings.CategoryModSetting)
                {
                    this.RefreshBindings(true);
                    this.controlCombo.ViewComponent.IsVisible = false;
                    this.controlText.ViewComponent.IsVisible = false;
                    this.label.IsVisible = true;
                    this.textLabel.IsVisible = false;
                    this.button.ViewComponent.IsVisible = false;
                    
                    return;
                }
                else if(this.modSetting is ModManagerModSettings.ButtonModSetting)
                {
                    this.RefreshBindings(true);
                    this.controlCombo.ViewComponent.IsVisible = false;
                    this.controlText.ViewComponent.IsVisible = false;
                    this.textLabel.IsVisible = true;
                    this.label.IsVisible = false;
                    this.button.ViewComponent.IsVisible = true;
                    this.button.Label = (this.modSetting as ModManagerModSettings.ButtonModSetting).GetButtonText();
                    this.button.Enabled = IsEnabled();

                    return;
                }
                else
                {
                    this.textLabel.IsVisible = true;
                    this.label.IsVisible = false;
                    this.button.ViewComponent.IsVisible = false;
                }

                if(!this.IsTextInput())
                    this.SetupOptions();
                else
                    this.controlText.Text = this.modSetting.GetValueAsString().formatted;

                this.RefreshBindings(true);
                this.controlCombo.ViewComponent.IsVisible = !this.IsTextInput();
                this.controlText.ViewComponent.IsVisible = this.IsTextInput();

                this.controlCombo.ViewComponent.Enabled = IsEnabled();
                this.controlText.ViewComponent.Enabled = IsEnabled();
            }

            this.CheckValue();
        }

        private bool IsEnabled()
        {
            if (this.modSetting == null)
                return false;

            return this.modSetting.CanBeDisabled() ? this.modSetting.IsEnabled() : true;
        }

        private void SetupOptions()
        {
            this.controlCombo.Elements.Clear();

            (string unformatted, string formatted)[] allowedValues = this.modSetting.GetAllowedValuesAsStrings();
            bool detectedSetting = false;

            int selectedIndex = 0;

            for (int index = 0; index < allowedValues.Length; index++)
            {
                this.controlCombo.Elements.Add(new ModOptionValue(allowedValues[index].formatted, allowedValues[index].unformatted));

                if(allowedValues[index] == this.modSetting.GetValueAsString())
                {
                    selectedIndex = index;
                    detectedSetting = true;
                }
            }

            this.controlCombo.MinIndex = 0;
            this.controlCombo.MaxIndex = allowedValues.Length;

            if (!detectedSetting)
                this.controlCombo.SelectedIndex = this.controlCombo.MinIndex;
            else
                this.controlCombo.SelectedIndex = selectedIndex;

            WrapField.SetValue(this.controlCombo, this.modSetting.GetWrap());
        }

        private bool IsTextInput()
        {
            return this.modSetting != null ? this.modSetting.GetAllowedValuesAsStrings() == null : true;
        }

        public override bool GetBindingValue(ref string _value, string _bindingName)
        {
            switch(_bindingName)
            {
                case "title":
                    _value = modSetting != null ? modSetting.unlocalizedName : "";
                    return true;
                default:
                    return false;
            }
        }

        private bool textSelected;

        private void ControlText_OnSelect(XUiController _sender, bool _selected)
        {
            textSelected = _selected;
            this.controlText.Text = _selected ? this.modSetting.GetValueAsString().unformatted : this.modSetting.GetValueAsString().formatted;
        }

        private void ControlText_OnChangeHandler(XUiController _sender, string _text, bool _changeFromCode)
        {
            if (!this.IsTextInput())
                return;

            bool flag = this.modSetting.SetValueFromString(_text);
            this.controlText.ActiveTextColor = !textSelected || flag ? Color.white : Color.red;
            
            if(flag)
            {
                this.CheckValue();
            }
        }

        private void ControlCombo_OnValueChanged(XUiController _sender, ModOptionValue _oldValue, ModOptionValue _newValue)
        {
            this.modSetting.SetValueFromString(_newValue.Value);

            this.CheckValue();
        }

        private void CheckValue()
        {
            bool flag = this.modSetting.IsDefault();
            this.controlText.ActiveTextColor = flag ? Color.white : Color.yellow;
            this.controlCombo.TextColor = flag ? Color.white : Color.yellow;
        }

        public readonly struct ModOptionValue
        {
            public readonly string Display;
            public readonly string Value;

            public ModOptionValue(string display, string value)
            {
                this.Display = display;
                this.Value = value;
            }

            public override string ToString()
            {
                return this.Display;
            }
        }
    }
}
