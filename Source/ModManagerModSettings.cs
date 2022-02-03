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

        internal static bool changed = false;
        private readonly Dictionary<string, string> loadedSettingsInstance;

        public ModManagerModSettings(Mod mod)
        {
            this.entry = CustomModManager.GetModEntryFromModInstance(mod);
            loadedSettingsInstance = loadedSettings.ContainsKey(this.entry.info.Name.Value) ? loadedSettings[this.entry.info.Name.Value] : new Dictionary<string, string>();

            modSettingsInstances.Add(entry, this);
        }

        public void Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T,string> toString, Func<string, (T, bool)> fromString, T[] allowedValues)
        {
            ModSetting<T> setting = new ModSetting<T>(nameUnlocalized, setCallback, getCallback, toString, fromString, allowedValues);
            settings.Add(key, setting);

            if(loadedSettingsInstance.ContainsKey(key))
            {
                setting.SetValueFromStringInternal(loadedSettingsInstance[key], true);
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

            internal abstract bool SetValueFromStringInternal(string str, bool loaded);
            
            public bool SetValueFromString(string str)
            {
                return SetValueFromStringInternal(str, false);
            }

            public abstract void Reset();

            public abstract string[] GetAllowedValuesAsStrings();

            public abstract Type GetValueType();

            internal abstract void SetLastValueInternal();
        }

        internal class ModSetting<T> : BaseModSetting
        {
            private readonly T defaultValue;
            private T savedValue;
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
                changed = !newValue.Equals(savedValue);
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

            internal override bool SetValueFromStringInternal(string str, bool loaded)
            {
                (T value, bool success) = fromString(str);

                Log.Out($"Load {success} from {str}");

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
