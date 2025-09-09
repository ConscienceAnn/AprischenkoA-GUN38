using System;
using System.IO;
using System.Text.Json;


namespace Game
{
    public class FileSystemSaveLoadService<T> : ISaveLoadService<T>
    {
        private readonly string _directoryPath;

       
        public FileSystemSaveLoadService(string directoryPath)
        {
            _directoryPath = directoryPath;
           
            if (!Directory.Exists(_directoryPath))
            {
                Directory.CreateDirectory(_directoryPath);
            }
        }

        public void SaveData(T data, string identifier)
        {
            
            string fullPath = Path.Combine(_directoryPath, identifier + ".txt");
            try
            {

                string json = JsonSerializer.Serialize(data);
                File.WriteAllText(fullPath, json);
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Ошибка при сохранении файла {identifier}: {ex.Message}");
            }
        }

        public T LoadData(string identifier)
        {
            string fullPath = Path.Combine(_directoryPath, identifier + ".txt");
            try
            {
            
                if (File.Exists(fullPath))
                {
                    string json = File.ReadAllText(fullPath);
                    return JsonSerializer.Deserialize<T>(json);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при загрузке файла {identifier}: {ex.Message}");
            }
            
            return default(T);
        }
    }
}
