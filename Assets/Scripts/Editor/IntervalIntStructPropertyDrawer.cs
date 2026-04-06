using UnityEditor;

namespace Netologia.Quest.Editor
{
	[CustomPropertyDrawer(typeof(IntervalInt))]
	public class IntervalIntStructPropertyDrawer : BaseOneLinePropertyDrawer
	{
		public IntervalIntStructPropertyDrawer() : base(2, .4f) {}
	}
}