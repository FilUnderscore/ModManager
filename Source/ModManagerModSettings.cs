using CustomModManager.API;
using System;
using System.Collections.Generic;
using System.Xml;

namespace CustomModManager
{
    public class ModManagerModSettings : IModSettings
    {
        internal static readonly Dictionary<ModEntry, ModManagerModSettings> modSettingsInstances = new Dictionary<ModEntry, ModManagerModSettings>();
        private readonly ModEntry entry;

        internal readonly Dictionary<string, BaseModSetting> settings = new Dictionary<string, BaseModSetting>();
        internal static readonly Dictionary<string, Dictionary<string, string>> loadedSettings = new Dictionary<string, Dictionary<string, string>>();

        internal readonly Dictionary<string, ModSettingTab> settingTabs = new Dictionary<string, ModSettingTab>();

        internal static readonly List<BaseModSetting> changed = new List<BaseModSetting>();
        private readonly Dictionary<string, string> loadedSettingsInstance;

        public ModManagerModSettings(Mod mod)
        {
            this.entry = ModLoader.GetModEntryFromModInstance(mod);
            loadedSettingsInstance = loadedSettings.ContainsKey(this.entry.info.Name.Value) ? loadedSettings[this.entry.info.Name.Value] : new Dictionary<string, string>();

            modSettingsInstances.Add(entry, this);
        }

        public IModSetting<T> Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T, (string, string)> toString, Func<string, (T, bool)> fromString) where T : IComparable<T>
        {
            if(settings.ContainsKey(key))
            {
                Log.Error($"[Mod Manager] [{this.entry.info.Name.Value}] A setting with key {key} already exists.");
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
                Log.Error($"[Mod Manager] [{this.entry.info.Name.Value}] A tab with key {key} already exists.");
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
            public readonly string unlocalizedName;
            
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
        }
    }
}
