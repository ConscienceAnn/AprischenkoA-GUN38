using System;
using UnityEngine;

namespace Netologia.Quest.Talks
{
	public abstract class BaseMessageSettings<TEnum> : ScriptableObject
		where TEnum : struct, Enum 
	{
		[field: SerializeField]
		public TEnum Type { get; private set; }
		[field: SerializeField]
		public Color Color { get; private set; } = Color.black;
		[field: SerializeField]
		public string[] Variants { get; private set; }
	}
}