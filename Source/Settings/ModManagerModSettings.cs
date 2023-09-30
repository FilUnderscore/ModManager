using CustomModManager.API;
using CustomModManager.Mod;
using CustomModManager.UI;
using System;
using System.Collections.Generic;

namespace CustomModManager
{
    public class ModManagerModSettings : IModSettings
    {
        internal static readonly Dictionary<Mod.Mod, ModManagerModSettings> modSettingsInstances = new Dictionary<Mod.Mod, ModManagerModSettings>();
        private readonly Mod.Mod entry;

        internal readonly Dictionary<string, BaseModSetting> settings = new Dictionary<string, BaseModSetting>();
        internal static readonly Dictionary<string, Dictionary<string, string>> loadedSettings = new Dictionary<string, Dictionary<string, string>>();

        internal readonly Dictionary<string, ModSettingTab> settingTabs = new Dictionary<string, ModSettingTab>();

        internal static readonly List<BaseModSetting> changed = new List<BaseModSetting>();
        private readonly Dictionary<string, string> loadedSettingsInstance;

        public ModManagerModSettings(global::Mod mod)
        {
            this.entry = ModLoader.Instance.GetModFromInstance(mod);

            loadedSettingsInstance = loadedSettings.ContainsKey(this.entry.Info.Name) ? loadedSettings[this.entry.Info.Name] : new Dictionary<string, string>();

            modSettingsInstances.Add(entry, this);
        }

        public IModSetting<string> Button(string key, string nameUnlocalized, Action clickCallback, Func<string> buttonText)
        {
            if (settings.ContainsKey(key))
            {
                Log.Error($"[Mod Manager] [{this.entry.Info.Name}] A setting with key {key} already exists.");
                return null;
            }

            ButtonModSetting setting = new ButtonModSetting(nameUnlocalized, clickCallback, buttonText);
            settings.Add(key, setting);

            return setting;
        }

        public IModSetting<string> Category(string key, string nameUnlocalized)
        {
            if(settings.ContainsKey(key))
            {
                Log.Error($"[Mod Manager] [{this.entry.Info.Name}] A setting with key {key} already exists.");
                return null;
            }

            CategoryModSetting setting = new CategoryModSetting(nameUnlocalized);
            settings.Add(key, setting);

            return setting;
        }

        public IModSetting<T> Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T, (string, string)> toString, Func<string, (T, bool)> fromString) where T : IComparable<T>
        {
            if(settings.ContainsKey(key))
            {
                Log.Error($"[Mod Manager] [{this.entry.Info.Name}] A setting with key {key} already exists.");
                return null;
            }

            ModSetting<T> setting = new ModSetting<T>(nameUnlocalized, setCallback, getCallback, toString, fromString);
            settings.Add(key, setting);

            if(loadedSettingsInstance.ContainsKey(key))
            {
                setting.SetValueFromStringInternal(loadedSettingsInstance[key], true);
            }

            return setting;
        }

        public void CreateTab(string key, string nameUnlocalized)
        {
            if(settingTabs.ContainsKey(key))
            {
                Log.Error($"[Mod Manager] [{this.entry.Info.Name}] A tab with key {key} already exists.");
                return;
            }

            settingTabs.Add(key, new ModSettingTab(nameUnlocalized));
        }

        public class ModSettingTab
        {
            public readonly string nameUnlocalized;

            public ModSettingTab(string nameUnlocalized)
            {
                this.nameUnlocalized = nameUnlocalized;
            }
        }

        public abstract class BaseModSetting
        {
            public XUiC_ModSettingSelector selector;
            public readonly string unlocalizedName;
            public Func<bool> enabled;

            public BaseModSetting(string unlocalizedName)
            {
                this.unlocalizedName = unlocalizedName;
            }

            public abstract (string unformatted, string formatted) GetValueAsString();

            internal abstract bool SetValueFromStringInternal(string str, bool loaded);
            
            public bool SetValueFromString(string str)
            {
                return SetValueFromStringInternal(str, false);
            }

            public abstract void Reset();

            public abstract (string unformattedString, string formattedString)[] GetAllowedValuesAsStrings();

            public abstract Type GetValueType();

            internal abstract void SetLastValueInternal();

            public abstract string GetTabKey();

            public abstract bool GetWrap();

            public abstract bool IsDefault();

            public abstract bool IsSerializable();

            public void Update()
            {
                if (selector == null)
                    return;

                selector.UpdateModSetting(this);
            }

            public void SetEnabled(Func<bool> enabled)
            {
                this.enabled = enabled;
            }

            public bool IsEnabled()
            {
                return this.enabled.Invoke();
            }

            public bool CanBeDisabled()
            {
                return this.enabled != null;
            }
        }

        internal sealed class ButtonModSetting : BaseModSetting, IModSetting<string>
        {
            private Action clickCallback;
            private Func<string> buttonText;

            private string tab;

            public ButtonModSetting(string unlocalizedName, Action clickCallback, Func<string> buttonText) : base(unlocalizedName)
            {
                this.clickCallback = clickCallback;
                this.buttonText = buttonText;
            }

            public Action GetClickCallback()
            {
                return this.clickCallback;
            }

            public string GetButtonText()
            {
                return this.buttonText.Invoke();
            }

            public override (string unformattedString, string formattedString)[] GetAllowedValuesAsStrings()
            {
                return null;
            }

            public override string GetTabKey()
            {
                return this.tab;
            }

            public override (string unformatted, string formatted) GetValueAsString()
            {
                throw new NotImplementedException();
            }

            public override Type GetValueType()
            {
                throw new NotImplementedException();
            }

            public override bool GetWrap()
            {
                throw new NotImplementedException();
            }

            public override bool IsDefault()
            {
                throw new NotImplementedException();
            }

            public override bool IsSerializable()
            {
                return false;
            }

            public override void Reset()
            {

            }

            public void SetAllowedValues(string[] values)
            {
                throw new NotImplementedException();
            }

            public void SetMinimumMaximumAndIncrementValues(string minimumValue, string maximumValue, string incrementValue)
            {
                throw new NotImplementedException();
            }

            public void SetTab(string tabKey)
            {
                this.tab = tabKey;
            }

            public void SetWrap(bool wrap)
            {
                throw new NotImplementedException();
            }

            internal override void SetLastValueInternal()
            {
                throw new NotImplementedException();
            }

            internal override bool SetValueFromStringInternal(string str, bool loaded)
            {
                throw new NotImplementedException();
            }
        }

        internal sealed class CategoryModSetting : BaseModSetting, IModSetting<string>
        {
            private string tab;
            
            public CategoryModSetting(string unlocalizedName) : base(unlocalizedName)
            {
            }

            public override (string unformattedString, string formattedString)[] GetAllowedValuesAsStrings()
            {
                return null;
            }

            public override string GetTabKey()
            {
                return this.tab;
            }

            public override (string unformatted, string formatted) GetValueAsString()
            {
                throw new NotImplementedException();
            }

            public override Type GetValueType()
            {
                throw new NotImplementedException();
            }

            public override bool GetWrap()
            {
                throw new NotImplementedException();
            }

            public override bool IsDefault()
            {
                throw new NotImplementedException();
            }

            public override void Reset()
            {

            }

            public void SetAllowedValues(string[] values)
            {
                throw new NotImplementedException();
            }

            public void SetMinimumMaximumAndIncrementValues(string minimumValue, string maximumValue, string incrementValue)
            {
                throw new NotImplementedException();
            }

            public void SetTab(string tabKey)
            {
                this.tab = tabKey;
            }

            public void SetWrap(bool wrap)
            {
                throw new NotImplementedException();
            }

            internal override void SetLastValueInternal()
            {
                throw new NotImplementedException();
            }

            internal override bool SetValueFromStringInternal(string str, bool loaded)
            {
                throw new NotImplementedException();
            }

            public override bool IsSerializable()
            {
                return false;
            }
        }

        internal class ModSetting<T> : BaseModSetting, IModSetting<T>
        {
            private readonly T defaultValue;
            private T savedValue;
            private T[] allowedValues;
            private string tab;

            private bool wrap;
            
            private Action<T> setCallback;
            private Func<T> getCallback;
            private Func<T, (string, string)> toString;
            private Func<string, (T, bool)> fromString;

            public ModSetting(string unlocalizedName, Action<T> setCallback, Func<T> getCallback, Func<T, (string, string)> toString, Func<string, (T, bool)> fromString) : base(unlocalizedName)
            {
                this.setCallback = setCallback;
                this.getCallback = getCallback;
                this.toString = toString;
                this.fromString = fromString;

                this.defaultValue = getCallback();
            }

            public void SetValue(T newValue)
            {
                if(!newValue.Equals(savedValue) && !changed.Contains(this))
                {
                    changed.Add(this);
                }
                else if(newValue.Equals(savedValue) && changed.Contains(this))
                {
                    changed.Remove(this);
                }

                setCallback(newValue);
            }

            public T GetValue()
            {
                return getCallback();
            }

            public override (string unformatted, string formatted) GetValueAsString()
            {
                return toString(GetValue());
            }

            internal override bool SetValueFromStringInternal(string str, bool loaded)
            {
                (T value, bool success) = fromString(str);

                if (success)
                {
                    SetValue(value);

                    if(loaded)
                    {
                        SetLastValueInternal();
                    }
                }

                return success;
            }

            internal override void SetLastValueInternal()
            {
                savedValue = GetValue();
            }

            public override (string unformattedString, string formattedString)[] GetAllowedValuesAsStrings()
            {
                if (allowedValues == null)
                    return null;

                List<(string, string)> values = new List<(string, string)>();

                foreach(var value in allowedValues)
                {
                    if (value == null)
                        continue;

                    values.Add(toString(value));
                }

                return values.ToArray();
            }

            public override void Reset()
            {
                SetValue(defaultValue);
            }

            public override Type GetValueType()
            {
                return typeof(T);
            }

            public void SetAllowedValues(T[] values)
            {
                if (values == null)
                    return;

                allowedValues = values;
            }

            public void SetTab(string tabKey)
            {
                this.tab = tabKey;
            }

            public override string GetTabKey()
            {
                return this.tab;
            }

            public void SetMinimumMaximumAndIncrementValues(T minimumValue, T maximumValue, T incrementValue)
            {
                if(!float.TryParse(toString(minimumValue).Item1, out float minimumValueAsFloat) ||
                    !float.TryParse(toString(maximumValue).Item1, out float maximumValueAsFloat) ||
                    !float.TryParse(toString(incrementValue).Item1, out float incrementValueAsFloat))
                    return;

                List<T> values = new List<T>();
                for(float v = minimumValueAsFloat; v < (maximumValueAsFloat + incrementValueAsFloat); v += incrementValueAsFloat)
                {
                    string str = v.ToString();

                    if(typeof(T) == typeof(int) && str.Contains("."))
                        str = str.Substring(0, str.IndexOf("."));

                    (T calculatedValue, bool success) = fromString(str);

                    if (success)
                        values.Add(calculatedValue);
                }

                SetAllowedValues(values.ToArray());
            }

            public void SetWrap(bool wrap)
            {
                this.wrap = wrap;
            }

            public override bool GetWrap()
            {
                return this.wrap;
            }

            public override bool IsDefault()
            {
                return this.defaultValue.Equals(this.GetValue());
            }

            public override bool IsSerializable()
            {
                return true;
            }
        }
    }
}
