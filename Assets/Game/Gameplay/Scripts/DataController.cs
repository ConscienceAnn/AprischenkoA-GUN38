using System.Linq;
using Game.GameEngine.Ecs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SampleProject
{
    public sealed class DataController : MonoBehaviour
    {
        [SerializeField]
        private Entity entity;

        [Button]
        public void MoveToPosition(Transform point)
        {
            this.entity.SetData(new MoveToPositionData
            {
                destination = point.position,
                stoppingDistance = 0.1f
            });
        }

        [Button]
        public void AttackTarget(Entity target)
        {
            this.entity.SetData(new AttackTarget
            {
                targetId = target.Id
            });
        }

        [Button]
        public void GatherResource(Entity resource)
        {
            this.entity.SetData(new GatherTarget
            {
                targetId = resource.Id
            });
        }

        [Button]
        public void Patrol(Transform[] points)
        {
            this.entity.SetData(new PatrolData
            {
                points = points.Select(it => it.position).ToList(),
                pointer = 0,
                stoppingDistance = 0.1f
            });
        }
    }
}