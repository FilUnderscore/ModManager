namespace CustomModManager.API
{
    public interface IModSetting<T>
    {
        void SetAllowedValues(T[] values);
        void SetTab(string tabKey);
    }
}
