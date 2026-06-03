using UnityEngine;

namespace Core.SaveLoad
{
    /// <summary>
    /// Save/Load in Player prefs
    /// </summary>
    public sealed class PlayerPrefsSaveLoadDataHandler : ISaveLoadDataHandler
    {
        public void SaveString(string key, string value) => PlayerPrefs.SetString(key,value);

        public bool TryLoadString(string key, out string value)
        {
            value = string.Empty;
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }
            value = PlayerPrefs.GetString(key);
            return true;
        }

        public void SaveInt(string key, int value) => PlayerPrefs.SetInt(key, value);

        public bool TryLoadInt(string key, out int value)
        {
            value = int.MinValue;
            if (!PlayerPrefs.HasKey(key))
            {
                return false;
            }
            value = PlayerPrefs.GetInt(key);
            return true;
        }

        public void ClearSavedData() => PlayerPrefs.DeleteAll();
    }
}