using Game.GameEngine.Ecs;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Entity))]
public class MoveAgent : MonoBehaviour
{
    private Entity unit;

    private void Awake()
    {
        this.unit = this.GetComponent<Entity>();
    }

    public void MoveToPosition(Vector3 destination)
    {
        // Только устанавливаем команду в ECS
        this.unit.SetData(new CommandRequest
        {
            type = CommandType.MOVE_TO_POSITION,
            args = destination,
            status = CommandStatus.IDLE
        });
    }

    public void StopMoving()
    {
        this.unit.RemoveData<CommandRequest>();
        this.unit.RemoveData<MoveToPositionData>();
        this.unit.RemoveData<MoveStepData>();
        this.unit.RemoveData<GroupMoveData>();
    }
}