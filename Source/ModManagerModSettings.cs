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

        public ModManagerModSettings(Mod mod)
        {
            this.entry = CustomModManager.GetModEntryFromModInstance(mod);
            this.Load();

            modSettingsInstances.Add(entry, this);
        }

        public void Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T,string> toString, Func<string, (T, bool)> fromString, T[] allowedValues)
        {
            settings.Add(key, new ModSetting<T>(nameUnlocalized, setCallback, getCallback, toString, fromString, allowedValues));
        }

        public void Load()
        {
            if (!loadedSettings.ContainsKey(this.entry.info.Name.Value))
                return;

            Dictionary<string, string> settingsAsStrings = loadedSettings[this.entry.info.Name.Value];

            foreach(var settingAsStringEntry in settingsAsStrings)
            {
                var settingKey = settingAsStringEntry.Key;
                var settingAsString = settingAsStringEntry.Value;
                
                if(settings.ContainsKey(settingKey))
                {
                    settings[settingKey].SetValueFromString(settingAsString);
                }
            }
        }

        public abstract class BaseModSetting
        {
            public readonly string unlocalizedName;
            
            public BaseModSetting(string unlocalizedName)
            {
                this.unlocalizedName = unlocalizedName;
            }

            public abstract string GetValueAsString();

            public abstract bool SetValueFromString(string str);

            public abstract void Reset();

            public abstract string[] GetAllowedValuesAsStrings();

            public abstract Type GetValueType();
        }

        internal class ModSetting<T> : BaseModSetting
        {
            private readonly T defaultValue;
            private readonly T[] allowedValues;

            private Action<T> setCallback;
            private Func<T> getCallback;
            private Func<T, string> toString;
            private Func<string, (T, bool)> fromString;

            public ModSetting(string unlocalizedName, Action<T> setCallback, Func<T> getCallback, Func<T, string> toString, Func<string, (T, bool)> fromString, T[] allowedValues) : base(unlocalizedName)
            {
                this.setCallback = setCallback;
                this.getCallback = getCallback;
                this.toString = toString;
                this.fromString = fromString;

                this.defaultValue = getCallback();
                this.allowedValues = allowedValues;
            }

            public void SetValue(T newValue)
            {
                setCallback(newValue);
            }

            public T GetValue()
            {
                return getCallback();
            }

            public override string GetValueAsString()
            {
                return toString(GetValue());
            }

            public override bool SetValueFromString(string str)
            {
                (T value, bool success) = fromString(str);
                
                if(success)
                    SetValue(value);

                return success;
            }

            public override string[] GetAllowedValuesAsStrings()
            {
                if (allowedValues == null)
                    return null;

                List<string> values = new List<string>();

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
        }
    }
}
