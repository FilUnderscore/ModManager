using System;

namespace CustomModManager.API
{
    public interface IModSettings
    {
        /// <summary>
        /// Hooks a variable to a mod unique key.
        /// </summary>
        void Hook<T>(string key, string nameUnlocalized, Action<T> setCallback, Func<T> getCallback, Func<T, string> toString, Func<string, (T, bool)> fromString, T[] allowedValues);
    }
}
