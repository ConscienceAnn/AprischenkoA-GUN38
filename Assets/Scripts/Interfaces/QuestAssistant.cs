using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Netologia.Quest.Characters;
using Netologia.Quest.Talks;

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Netologia.Quest.Interfaces
{
	// Визуалка квестов и диалогов
	public class QuestAssistant : MonoBehaviour
	{
		[Serializable]
		private struct ProgressBar
		{
			private const string c_pattern = "{0} / {1} ({2} %)";
			
			[SerializeField]
			private TextMeshProUGUI _progressTextEmpty;
			[SerializeField]
			private TextMeshProUGUI _progressTextFill;
			[SerializeField]
			private Image _bar;

			public int Current { get; private set; }
			public int Max { get; private set; }
			
			public void Append()
			{
				Current++;
				var percent = (float)Math.Round((double)Current / Max, 1);
				_bar.fillAmount = percent;
				var str = string.Format(c_pattern, Current.ToString(), Max.ToString(), (100f * percent).ToString());
				_progressTextEmpty.text = str;
				_progressTextFill.text = str;
			}
			
			public void Init(int max)
			{
				Max = max;
				Current = -1;
				Append();
			}
		}

		[Serializable]
		private struct CompletePush
		{
			[SerializeField]
			private GameObject _push;
			[SerializeField]
			private TextMeshProUGUI _description;
			[SerializeField]
			private RectTransform _check;
			[SerializeField]
			private float _grow;
			[SerializeField]
			private float _decrease;
			[SerializeField]
			private float _stay;
			
			public IEnumerator SendPush(string description)
			{
				_description.text = description;
				_push.SetActive(true);

				var delay = _grow;
				//grow 0 -> 2
				while (delay > 0f)
				{
					_check.localScale = Vector3.one * Mathf.Lerp(0f, 2f, (_grow - delay) / _grow);
					delay -= TimeManager.DeltaTime;
					yield return null;
				}

				delay = _decrease;
				//decrease 2 -> 1
				while (delay > 0f)
				{
					_check.localScale = Vector3.one * Mathf.Lerp(2f, 1f, (_decrease - delay) / _decrease);
					delay -= TimeManager.DeltaTime;
					yield return null;
				}
				
				delay = _stay;
				_check.localScale = Vector3.one;
				//stay 1
				while (delay > 0f)
				{
					delay -= TimeManager.DeltaTime;
					yield return null;
				}
				
				_push.SetActive(false);
				//_pushCoroutine = null;
				//надо всё сделать через системный класс ивентов, который всех и обо всём опповещает - переделай Диман плз
			}
			
			public CompletePush(float grow, float decrease, float stay)
				=> (_check, _description, _push, _grow, _decrease, _stay)
					= (default, default, default, grow, decrease, stay);
		}

		private struct ActiveQuestData : IEquatable<ActiveQuestData>
		{
			public TextMeshProUGUI Text;
			public QuestInfo Source;
			
			public static implicit operator ActiveQuestData((TextMeshProUGUI text, QuestInfo source) pair)
				=> new() { Text = pair.text, Source = pair.source };

			public static bool operator ==(ActiveQuestData a, QuestInfo b)
				=> a.Source == b;
			public static bool operator !=(ActiveQuestData a, QuestInfo b)
				=> a.Source != b;
			public bool Equals(ActiveQuestData other)
				=> Equals(Source, other.Source);
			public override bool Equals(object obj)
				=> obj is ActiveQuestData other && Equals(other);
			public override int GetHashCode()
				=> HashCode.Combine(Text, Source);
		}

		[Serializable]
		private struct Dialog
		{
			private const string Set = "---New Quest---";
			private const string Get = "---Complete Quest---";
			
			[SerializeField]
			private GameObject _dialog;
			[SerializeField]
			private TextMeshProUGUI _name;
			[SerializeField]
			private TextMeshProUGUI _text;
			[SerializeField]
			private Button _closeButton;

			public void SetDialog(string text, bool completed)
			{
				_text.text = text;
				_name.text = completed ? Get : Set;
				_dialog.SetActive(true);
			}

			public void Close()
				=> _dialog.SetActive(false);

			public void Registry(UnityAction eventAction)
				=> _closeButton.onClick.AddListener(eventAction);

			public void Unregistry()
				=> _closeButton.onClick.RemoveAllListeners();
		}

		private Coroutine _pushCoroutine;
		
		private readonly List<ActiveQuestData> _inactiveQuestList = new (8);
		private readonly List<ActiveQuestData> _activeQuestList = new (8);
		
		private TextMeshProUGUI _namePanel;
		private bool _nameFocus;
		
		[SerializeField]
		private ProgressBar _progress;
		[SerializeField]
		private CompletePush _push = new (3f, .5f, 2f);
		[SerializeField]
		private Dialog _dialog;
		
		[SerializeField]
		private TextMeshProUGUI _activeQuestDescriptionPrefab;
		[SerializeField]
		private RectTransform _listQuestPanel;
		[SerializeField, Space, Tooltip("Плашка с названием объекта, следующим за мышкой")]
		private RectTransform _nameRect;
		[SerializeField]
		private Vector2 _nameOffset = new (15f, 15f);
		
		private void Start()
		{
			_namePanel = _nameRect.GetComponentInChildren<TextMeshProUGUI>();
			_nameRect.gameObject.SetActive(false);
			
			InformationBureau.OnQuestChanged += OnCharacterQuestChangedHandler;
			InformationBureau.OnCharacterFocused += CharacterOnFocusHandler;
			InformationBureau.OnShowDialog += ShowDialog;
			
			_dialog.Registry(() =>
			{
				_dialog.Close();
				InformationBureau.CallOnDialogClose();
			});
			_dialog.Close();
			//при условии, что сотрудники не исчезают и не появляются
			_progress.Init(FindObjectsOfType<Character>().Sum(character => character.QuestCount));
			FillTextPool();
		}
		private void Update()
		{
			if (_nameFocus)
				_nameRect.position = Mouse.current.position.ReadValue() + _nameOffset;
		}

		public void ShowDialog(string text, bool completed)
			=> _dialog.SetDialog(text, completed);
		
		private void OnCharacterQuestChangedHandler(QuestInfo quest)
		{
			var data = default(ActiveQuestData);
			switch (quest.State)
			{
				case QuestInfo.Status.Progress:
					if(_inactiveQuestList.Count == 0)
						FillTextPool();
					data = _inactiveQuestList[^1];
					_inactiveQuestList.RemoveAt(_inactiveQuestList.Count - 1);

					data.Text.gameObject.SetActive(true);
					data.Text.transform.SetAsLastSibling();
					data.Source = quest;
					data.Text.text = quest.Short;
					_activeQuestList.Add(data);
					break;
				case QuestInfo.Status.Complete:
					//Update active list
					var num = _activeQuestList.FindIndex(t=> t == quest);
					data = _activeQuestList[num];
					_activeQuestList.RemoveAt(num);
					_inactiveQuestList.Add(data);
					data.Text.gameObject.SetActive(false);
					data.Source = default;
					//Update progress
					_progress.Append();
					
					if(_pushCoroutine is not null)
						StopCoroutine(_pushCoroutine);
					_pushCoroutine = StartCoroutine(_push.SendPush(data.Text.text));
					
					//todo finish win logic if need
#if UNITY_EDITOR
					if (_progress.Current == _progress.Max)
					{
						UnityEditor.EditorApplication.isPaused = true;
						Debug.Log("Finish Game!");
					}
#endif
					break;
				case QuestInfo.Status.New:
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void FillTextPool()
		{
			for (int i = 0, iMax = _inactiveQuestList.Capacity; i < iMax; i++)
			{
				var text = Instantiate(_activeQuestDescriptionPrefab, _listQuestPanel);
				text.gameObject.SetActive(false);
				_inactiveQuestList.Add((text, default));
			}
		}
		
		private void CharacterOnFocusHandler(bool activation, string name)
		{
			if (activation)
			{
				_namePanel.text = name;
				_nameRect.gameObject.SetActive(_nameFocus = true);
			}
			else
				_nameRect.gameObject.SetActive(_nameFocus = false);
		}
		
		private void OnDestroy()
		{
			InformationBureau.OnQuestChanged -= OnCharacterQuestChangedHandler;
			InformationBureau.OnCharacterFocused -= CharacterOnFocusHandler;
			InformationBureau.OnShowDialog -= ShowDialog;
		}
	}
}