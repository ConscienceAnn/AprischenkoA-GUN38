using UnityEngine;
using static MoveAgent;

public class Entity : MonoBehaviour
{
    public float moveSpeed = 5f;

    private MoveStateComponent currentMoveState;

    public void SetData(MoveStateComponent data)
    {
        currentMoveState = data;
    }

    private void Update()
    {
        if (currentMoveState.moveRequired)
        {
            transform.Translate(currentMoveState.direction * moveSpeed * Time.deltaTime, Space.World);

            if (currentMoveState.direction != Vector3.zero)
            {
                transform.forward = currentMoveState.direction;
            }
        }
    }
}