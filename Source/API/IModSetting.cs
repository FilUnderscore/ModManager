using System;

namespace CustomModManager.API
{
    public interface IModSetting<T>
    {
        void SetAllowedValues(T[] values);
        void SetTab(string tabKey);
        void SetMinimumMaximumAndIncrementValues(T minimumValue, T maximumValue, T incrementValue);
        void SetWrap(bool wrap);
    }
}
