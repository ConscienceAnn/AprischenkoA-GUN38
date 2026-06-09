using System.Linq;
using Game.GameEngine.Ecs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SampleProject
{
    //TEST
    public sealed class CommandController : MonoBehaviour
    {
        [SerializeField]
        private Entity entity;

        [Button]
        public void MoveToPosition(Transform point)
        {
            this.entity.SetData(new CommandRequest
            {
                type = CommandType.MOVE_TO_POSITION,
                args = point.position,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void AttackTarget(Entity target)
        {
            this.entity.SetData(new CommandRequest
            {
                type = CommandType.ATTACK_TARGET,
                args = target,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void GatherResource(Entity resource)
        {
            this.entity.SetData(new CommandRequest
            {
                type = CommandType.GATHER_RESOURCE,
                args = resource,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void Patrol(Transform[] points)
        {
            this.entity.SetData(new CommandRequest
            {
                type = CommandType.PATROL_BY_POINTS,
                args = points.Select(it => it.position).ToList(),
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void Stop()
        {
            this.entity.RemoveData<CommandRequest>();
        }
    }

    
    
    
}