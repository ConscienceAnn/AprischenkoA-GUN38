using Netologia.Quest.Characters;

using System;

using UnityEngine;

namespace Netologia.Quest
{
	[Serializable]
	public class QuestInfo
	{
		public enum Status : byte
		{
			New,
			Progress,
			Complete
		}

		[HideInInspector]
		public Status State;

		public Character Target;
		public string Short;
		public string Question;
		public string Answer;

		public override string ToString()
			=> $"{Target.Name} Status: {State}";
	}
}