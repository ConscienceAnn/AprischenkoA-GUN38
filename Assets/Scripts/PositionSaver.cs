using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DefaultNamespace
{
	public class PositionSaver : MonoBehaviour
	{
		[Serializable]
		public struct Data
		{
			public Vector3 Position;
			public float Time;
		}


        [Tooltip("Для заполнения этого поля используйте контекстное меню → 'Create File'")]
        [ReadOnly]
        [SerializeField]
        private TextAsset _json;

		[field: SerializeField, HideInInspector]
		public List<Data> Records { get; private set; }

		private void Awake()
		{
            //todo comment: Что будет, если в теле этого условия не сделать выход из метода?
            // Если не сделать return, то после вывода сообщения об ошибке выполнение метода продолжится, и программа попытается выполнить следующую строку JsonUtility.FromJsonOverwrite(_json.text, this). Что приведет к ошибке, краш программы. 
            if (_json == null)
			{
				//gameObject.SetActive(false);
				Debug.LogError("Please, create TextAsset and add in field _json");
				return;
			}
			

			JsonUtility.FromJsonOverwrite(_json.text, this);
			//todo comment: Для чего нужна эта проверка (что она позволяет избежать)?
			// проверяет существует ли список, есть ли куда записывать. Позволяет избежать ошибки при попытке впихать в никуда. 
			if (Records == null)
				Records = new List<Data>(10);
		}

		private void OnDrawGizmos()
		{
            //todo comment: Зачем нужны эти проверки (что они позволляют избежать)?
            //Если данных нет (null) или они пустые (Count == 0) - просто выходим из метода, не пытаемся рисовать то чего нет
            if (Records == null || Records.Count == 0) return;
			var data = Records;
			var prev = data[0].Position;
			Gizmos.color = Color.green;
			Gizmos.DrawWireSphere(prev, 0.3f);
            //todo comment: Почему итерация начинается не с нулевого элемента?
            //рисуем линии между точками: от точки 0 к точке 1, от точки 1 к точке 2, и т.д.
            //всегда две точки: предыдущая и текущая
            //Первая точка уже обработана (var prev = data[0].Position;), поэтому цикл начинается со второй точки(i = 1)

            for (int i = 1; i < data.Count; i++)
			{
				var curr = data[i].Position;
				Gizmos.DrawWireSphere(curr, 0.3f);
				Gizmos.DrawLine(prev, curr);
				prev = curr;
			}
		}
		
#if UNITY_EDITOR
		[ContextMenu("Create File")]
		private void CreateFile()
		{
            //todo comment: Что происходит в этой строке?
            //Создается новый файл "Path.txt" в папке Assets проекта
            var stream = File.Create(Path.Combine(Application.dataPath, "Path.txt"));
            //todo comment: Подумайте для чего нужна эта строка? (а потом проверьте догадку, закомментировав) 
            //Чтобы освободить файл и снять с него блокировку. Без этой строки файл останется занятым процессом Unity, и дальнейшие манипуляции с ним будут невозможны.
            stream.Dispose();
			UnityEditor.AssetDatabase.Refresh();
			//В Unity можно искать объекты по их типу, для этого используется префикс "t:"
			//После нахождения, Юнити возвращает массив гуидов (которые в мета-файлах задаются, например)
			var guids = UnityEditor.AssetDatabase.FindAssets("t:TextAsset");
			foreach (var guid in guids)
			{
				//Этой командой можно получить путь к ассету через его гуид
				var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
				//Этой командой можно загрузить сам ассет
				var asset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //todo comment: Для чего нужны эти проверки?
                // проверяем, что ассет действительно существует и не был удален
                //ищет именно тот файл, который мы создали ("Path.txt"), а не любой TextAsset
                if (asset != null && asset.name == "Path")
				{
					_json = asset;
					UnityEditor.EditorUtility.SetDirty(this);
					UnityEditor.AssetDatabase.SaveAssets();
					UnityEditor.AssetDatabase.Refresh();
                    //todo comment: Почему мы здесь выходим, а не продолжаем итерироваться?
                    //нужный файл "Path.txt" найден. Дальнейший поиск среди остальных TextAsset бессмысленен и будет только тратить ресурсы.
                    return;
				}
			}
		}

		private void OnDestroy()
		{
            if (_json == null || Records == null || Records.Count == 0)
                return;

            // Сохраняем весь компонент — но _json не попадёт в JSON, если объявлено правильно
            string jsonString = JsonUtility.ToJson(this, true);
            string filePath = UnityEditor.AssetDatabase.GetAssetPath(_json);
            File.WriteAllText(filePath, jsonString);
            UnityEditor.AssetDatabase.Refresh();
            Debug.Log($"Records saved to {_json.name}");
        }
#endif
	}
}