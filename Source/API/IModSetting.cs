using System;

namespace CustomModManager.API
{
    public interface IModSetting<T> where T : IComparable<T>
    {
        void SetAllowedValues(T[] values);
        void SetTab(string tabKey);
        void SetMinimumValue(T value);
        void SetMaximumValue(T value);
        void SetIncrementValue(T value);
        void SetWrap(bool wrap);
    }
}
