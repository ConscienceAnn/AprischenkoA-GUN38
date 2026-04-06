using System;
using System.Collections.Generic;
using System.Linq;
using Netologia.Quest.Talks;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Netologia.Quest
{
	public class Director : MonoBehaviour
	{
		public static MessageData Personal { get; private set; }
		public static MessageData Work { get; private set; }

		public readonly struct MessageData
		{
			private readonly IReadOnlyList<int> _values;
			private readonly IReadOnlyDictionary<int, (Color Font, string[] Variants)> _dictionary;
			
			public (Color Font, string Text) this[int flag]
			{
				get
				{
					//В Юнити -1 означает все значения, в C# у флага все значения это MinValue
					if (flag == -1) flag = int.MinValue;

					var count = _values.Count;
					var index = default(int);
					//todo чем меньше значений у персонажа, тем чаще промах и повторная прокрутка цикла
					do index = _values[Random.Range(0, count)];
					//Если равно нулю - бот не имеет такое значение в флаге
					while((flag & index) != 0);

					if (_dictionary.TryGetValue(index, out var data))
						return (data.Font, data.Variants[Random.Range(0, data.Variants.Length)]);
			
					Debug.LogError($"Нет сообщения в словаре рабочих сообщений под индексом: {(WorkerFeatureFlag)index}");
					return default;
				}
			}

			public static MessageData Create<T>(BaseMessageSettings<T>[] messages, Array enumer)
				where T : struct, Enum
			{
				var list = new List<string>(512);
				var dic = new Dictionary<int, (Color Font, string[] Variants)>(enumer.Length);
				var color = default(Color);
				foreach (T value in enumer)
				{
					for (int i = 0, iMax = messages.Length; i < iMax; i++)
					{
						if (messages[i].Type.Equals(value)) //boxing(((
						{
							list.AddRange(messages[i].Variants);
							color = messages[i].Color;
						}
					}

					if (list.Count == 0) continue;
					dic[UnsafeUtility.EnumToInt(value)] = (color, list.ToArray());
					list.Clear();
				}

				return new MessageData(dic);
			}
			
			private MessageData(IReadOnlyDictionary<int, (Color Font, string[] Variants)> dictionary)
				=> (_dictionary, _values) = (dictionary, dictionary.Keys.ToArray());
		}
		
		[SerializeField, Tooltip("Наборы случайных репплик по работе")]
		private WorkMessageSettings[] _workMessages;
		[SerializeField, Tooltip("Набор случайных реплик по дружбе персонажей")]
		private CharacterMessageSettings[] _characterMessages;

		private void Awake()
		{
			TimeManager.IsGame = true;

			Personal = MessageData.Create(_characterMessages, Enum.GetValues(typeof(CharacterFeatureFlag)));
			Work = MessageData.Create(_workMessages, Enum.GetValues(typeof(WorkerFeatureFlag)));
		}

		private void Update()
		{
			TimeManager.IncrementDeltaTime();
		}
	}
}