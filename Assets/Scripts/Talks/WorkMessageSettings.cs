using UnityEngine;

namespace Netologia.Quest.Talks
{
	[CreateAssetMenu(menuName = "Quest/Work Messages", fileName = "Quest/NewWorkMessages", order = 0)]
	public class WorkMessageSettings : BaseMessageSettings<WorkerFeatureFlag> { }

	[System.Flags]
	public enum WorkerFeatureFlag
	{
		None = 0,
		Beginner = 1 << 0,
		June = 1 << 1,
		Middle = 1 << 2,
		Senior = 1 << 3,
		Angry = 1 << 4,
		Calm = 1 << 5,
	}
}