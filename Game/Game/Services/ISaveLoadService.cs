

namespace Game
{
    public interface ISaveLoadService<T>
    {

        void SaveData(T data, string identifier);
        T LoadData(string identifier);
    }
}
