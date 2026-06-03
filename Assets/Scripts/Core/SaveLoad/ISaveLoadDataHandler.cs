namespace Core.SaveLoad
{
    /// <summary>
    /// Implement this for custom save/load of standard types
    /// </summary>
    public interface ISaveLoadDataHandler
    {
        void SaveString(string key, string value);

        bool TryLoadString(string key, out string value);

        void SaveInt(string key, int value);

        bool TryLoadInt(string key, out int value);

        void ClearSavedData();
    }
}