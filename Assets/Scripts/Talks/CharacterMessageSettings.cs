using UnityEngine;

namespace Netologia.Quest.Talks
{
	[CreateAssetMenu(menuName = "Quest/Character Messages", fileName = "Quest/NewCharacterMessages", order = 1)]
	public class CharacterMessageSettings : BaseMessageSettings<CharacterFeatureFlag> { }

	[System.Flags]
	public enum CharacterFeatureFlag
	{
		None = 0,
		Stranger = 1 << 0,
		Friend = 1 << 1,
	}
}