using System;
using Netologia.Quest.Characters.Player;
using Netologia.Quest.Talks;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Netologia.Quest.Characters
{
	public class Character : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
	{
		private MessagePusher _pusher;
		private Transform _circleTrans;

		[SerializeField]
		private float _interactRadius = 2f;
		[SerializeField, Space]
		private string _objectName;
		[SerializeField]
		private MeshRenderer _circle;

		[SerializeField]
		private QuestInfo[] _quests;

		public int QuestCount => _quests.Length;
		public bool MoveAccess { get; private set; } = true;

		public Vector3 CirclePosition => _circleTrans.position;
		public string Name => _objectName;
		
		private void Start()
		{
			_pusher = GetComponent<MessagePusher>();
			InformationBureau.OnDialogClose += OnCancel;
			
			if (string.IsNullOrEmpty(_objectName))
				_objectName = name;
			
			_circle.enabled = false;
			_circleTrans = _circle.transform;
		}
		
		public bool OnInteract(PlayerController controller)
		{
			if (TrySetQuest(controller) || TryGetQuest(controller))
			{
				//In dialog character can't move
				MoveAccess = false;
				_pusher.LockMessage(true);
				return true;
			}
			_pusher.CharacterPush();
			return false;
		}

		private bool TrySetQuest(PlayerController controller)
		{
			foreach (var quest in _quests)
			{
				switch (quest.State)
				{
					//Выдаем новый квест
					case QuestInfo.Status.New:
						quest.State = QuestInfo.Status.Progress;
						//todo add dialog text?
						InformationBureau.CallOnShowDialog(quest.Question, false);
						InformationBureau.CallOnQuestChanged(quest);
						controller.ActiveQuests.Add(quest);
						return true;
					//Не выдаем новый квест, пока есть в прогрессе
					case QuestInfo.Status.Progress:
						return false;
					//Пропускаем выполненные квесты
					case QuestInfo.Status.Complete:
						continue;
					default:
						throw new ArgumentOutOfRangeException();
				}
			}

			return false;
		}

		private bool TryGetQuest(PlayerController controller)
		{
			for(int i = 0, iMax = controller.ActiveQuests.Count; i < iMax; i++)
			{
				var quest = controller.ActiveQuests[i];
				if(quest.Target != this) continue;
				quest.State = QuestInfo.Status.Complete;
				//todo add dialog text?
				InformationBureau.CallOnShowDialog(quest.Answer, true);
				InformationBureau.CallOnQuestChanged(quest);
				controller.ActiveQuests.RemoveAt(i);
				return true;
			}

			return false;
		}
		
		private void OnCancel()
		{
			//Only talking bot
			if (MoveAccess) return;
			MoveAccess = true;
			_pusher.LockMessage(false);
		}
		
		void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
		{//можно отсюда подцепать таргет, если пол тоже сделать взаимодействуемым
			_circle.enabled = true;
			InformationBureau.CallOnCharacterFocused(true, _objectName);
		}

		void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
		{
			_circle.enabled = false;
			InformationBureau.CallOnCharacterFocused(false, _objectName);
		}
		
		private void OnDestroy()
		{
			InformationBureau.OnDialogClose -= OnCancel;
		}
		
		private void OnDrawGizmos()
		{
			Gizmos.color = Color.magenta;
			//Gizmos.DrawWireSphere(transform.position, _interactRadius);
		}
	}
}