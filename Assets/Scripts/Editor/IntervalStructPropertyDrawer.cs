using UnityEditor;

namespace Netologia.Quest.Editor
{
    [CustomPropertyDrawer(typeof(Interval))]
    public class IntervalStructPropertyDrawer : BaseOneLinePropertyDrawer
    {
        public IntervalStructPropertyDrawer() : base(2, .4f) {}
    }
}