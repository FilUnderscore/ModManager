using System;

namespace CustomModManager.API
{
    public interface IModSettings
    {
        /// <summary>
        /// Hooks a variable to a mod unique key.
        /// </summary>
        /// <typeparam name="T">Variable type</typeparam>
        /// <param name="key">Setting key</param>
        /// <param name="callback">Callback</param>
        void Hook<T>(string key, string nameUnlocalized, string tooltip, Action<T> setCallback, Func<T> getCallback, Func<T, string> toString, Func<string, (T, bool)> fromString, T[] allowedValues);
    }
}
