using System.Collections;
using System;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(Entity))]
public class MoveAgent : MonoBehaviour
{
    private const float stopping_distance_sqr = 0.25f;
    private const float complete_delay = 0.1f;
    private const float correct_path_period = 0.75f;
    private const float shift_offset = 2.0f;
    private const float shift_factor = 0.75f;

    private Entity unit;
    private Vector3 destination;
    private NavMeshPath navMeshPath;

    private Coroutine moveCoroutine;
    private Coroutine completeCoroutine;
    private Coroutine checkObstacleCoroutine;
    private Coroutine avoidObstacleCoroutine;

    private Vector3[] pointPath;
    private int pointer;
    private bool isCompleted;
    private float correctPathTime;

    public bool IsCompleted
    {
        get { return this.isCompleted; }
    }

    public bool IsObstacleAvoid
    {
        get { return this.avoidObstacleCoroutine != null; }
    }

    public bool CanCorrectPath
    {
        get { return Time.time - this.correctPathTime >= correct_path_period; }
    }

    public bool IsLastPoint
    {
        get { return this.pointer >= this.pointPath.Length - 1; }
    }

    private void Awake()
    {
        this.unit = this.GetComponent<Entity>();
        this.navMeshPath = new NavMeshPath();
    }

    #region Move

    public void MoveToPosition(Vector3 destination)
    {
        this.StopMove(isCompleted: false);
        this.StartMove(destination);
    }

    private void StartMove(Vector3 destination)
    {
        this.destination = destination;

        var pathGenerated = NavMesh.CalculatePath(

            this.transform.position,
            this.destination,
            NavMesh.AllAreas,
            this.navMeshPath
        );

        if (!pathGenerated)
        {
            return;
        }

        this.pointer = 0;
        this.pointPath = this.navMeshPath.corners;

        this.moveCoroutine = this.StartCoroutine(this.MoveRoutine());
        this.checkObstacleCoroutine = this.StartCoroutine(this.CheckObstacleRoutine());
    }

    private IEnumerator MoveRoutine()
    {
        var framePeriod = new WaitForFixedUpdate();
        while (this.pointer < this.pointPath.Length)
        {
            yield return framePeriod;

            if (this.IsObstacleAvoid)
            {
                continue;
            }

            this.MoveByPath();
        }

        this.StopMove(isCompleted: true);
    }

    public struct MoveStateComponent
    {
        public bool moveRequired;
        public Vector3 direction;

        public MoveStateComponent(bool moveRequired, Vector3 direction)
        {
            this.moveRequired = moveRequired;
            this.direction = direction;
        }
    }
    private void MoveByPath()
    {
        var currentPosition = this.transform.position;
        var targetPosition = this.pointPath[this.pointer];
        var distanceVector = targetPosition - currentPosition;

        var isTargetReached = distanceVector.sqrMagnitude <= stopping_distance_sqr;
        if (isTargetReached)
        {
            this.pointer++;
            return;
        }

        if (IsPathBlocked())
        {
            // ќстанавливаем движение
            this.unit.SetData(new MoveStateComponent(false, Vector3.zero));
            return;
        }

        var direction = distanceVector.normalized;
        this.MoveUnit(direction);
    }

    private void MoveUnit(Vector3 direction)
    {
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        this.unit.SetData(new MoveStateComponent(true, direction));
    }

    #endregion

    #region CorrectPath

    public void CorrectPath()
    {
        if (!this.CanCorrectPath || this.IsLastPoint)
        {
            return;
        }

        this.correctPathTime = Time.time;

        var currentPosition = this.transform.position;
        var targetPosition = this.pointPath[this.pointer];
        var nextPosition = this.pointPath[this.pointer + 1];

        var line = nextPosition - currentPosition;

        var isRight = Algorithms.PointRelativeToVector(currentPosition, nextPosition, targetPosition) > 0;
        var crossVector = isRight ? Vector3.up : Vector3.down;
        var shiftOffset = Vector3.Cross(line.normalized, crossVector) * shift_offset;

        var newPosition = Vector3.Lerp(
            currentPosition + shiftOffset,
            nextPosition - shiftOffset,
            shift_factor
            );

        if (NavMesh.SamplePosition(newPosition, out var hit, 2.0f, NavMesh.AllAreas))
        {
            newPosition = hit.position;
        }


        var pathGenerated = NavMesh.CalculatePath(
            newPosition,
            this.destination,
            NavMesh.AllAreas,
            this.navMeshPath
            );


        if (!pathGenerated)
        {
            this.pointPath[this.pointer] = newPosition;
            return;
        }

        this.pointer = 0;
        this.pointPath = this.navMeshPath.corners;
    }


    public bool TryGetNextPosition(out Vector3 targetPosition)
    {
        var lastPoint = this.pointer >= this.pointPath.Length - 1;

        if (lastPoint)
        {
            targetPosition = default;
            return false;
        }

        targetPosition = this.pointPath[this.pointer];
        return true;
    }

    #endregion

    #region ObstacleAvoidance

    public void StartAvoidObstacle()
    {
        if (this.avoidObstacleCoroutine == null)
        {
            var avoidDirection = Vector3.Cross(this.transform.forward, Vector3.up);
            this.avoidObstacleCoroutine = this.StartCoroutine(this.AvoidObstacleRoutine(avoidDirection));
        }
    }

    private void StartAvoidObstacle(Vector3 avoidDirection)
    {
        if (this.avoidObstacleCoroutine == null)
        {
            this.avoidObstacleCoroutine = this.StartCoroutine(this.AvoidObstacleRoutine(avoidDirection));
        }
    }

    private void StopAvoidObstacle()
    {
        if (this.avoidObstacleCoroutine != null)
        {
            this.StopCoroutine(this.avoidObstacleCoroutine);
            this.avoidObstacleCoroutine = null;
        }
    }

    private IEnumerator AvoidObstacleRoutine(Vector3 moveDirection)
    {
        while (true)
        {
            yield return new WaitForFixedUpdate();
            this.unit.SetData(new MoveStateComponent
            {
                moveRequired = true,
                direction = moveDirection
            });
        }
    }


    private IEnumerator CheckObstacleRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.35f);

            var currentPosition = this.transform.position;
            var targetPosition = this.pointPath[this.pointer];
            var direction = (targetPosition - currentPosition).normalized;

            var ray = new Ray(currentPosition, direction); // заменила this.
            if (!Physics.Raycast(ray, out var hit, 0.35f, LayerMask.GetMask("Obstacle")))
            {
                this.StopAvoidObstacle();
            }
            else
            {
                var avoidDirection = Vector3.Cross(hit.normal, Vector3.up);
                this.StartAvoidObstacle(avoidDirection);
            }
        }
    }

    #endregion

    #region Stop

    public void CompleteMove()
    {
        if (this.isCompleted || this.completeCoroutine != null)
        {
            return;
        }

        this.completeCoroutine = this.StartCoroutine(this.CompleteDelayed());
    }

    private IEnumerator CompleteDelayed()
    {
        yield return new WaitForSeconds(complete_delay);

        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // ѕолна€ остановка перед финишем
        this.unit.SetData(new MoveStateComponent(false, Vector3.zero));

        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        this.StopMove(isCompleted: true);
    }


    private void StopMove(bool isCompleted)
    {
        if (this.moveCoroutine != null)
        {
            this.StopCoroutine(this.moveCoroutine);
            this.moveCoroutine = null;
        }

        if (this.checkObstacleCoroutine != null)
        {
            this.StopCoroutine(this.checkObstacleCoroutine);
            this.checkObstacleCoroutine = null;
        }

        if (this.completeCoroutine != null)
        {
            this.StopCoroutine(this.completeCoroutine);
            this.completeCoroutine = null;
        }

        if (this.avoidObstacleCoroutine != null)
        {
            this.StopCoroutine(this.avoidObstacleCoroutine);
            this.avoidObstacleCoroutine = null;
        }

        this.unit.SetData(new MoveStateComponent(false, Vector3.zero));

        transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);

        // ѕринудительно ставим на землю
        Vector3 pos = transform.position;
        pos.y = 0;
        transform.position = pos;

        this.isCompleted = isCompleted;
    }

    public void StopMoving()
    {
        StopMove(false);
    }

    public bool IsPathBlocked()
    {
        // ≈сли мы уже почти у цели - не блокируем
        if (Vector3.Distance(transform.position, destination) <= 0.5f)
            return false;

        // ѕровер€ем, есть ли кто-то впереди на пути
        RaycastHit hit;
        Vector3 forward = (destination - transform.position).normalized;

        if (Physics.Raycast(transform.position, forward, out hit, 1.5f)) // 1.5f - дистанци€ проверки
        {
            if (hit.collider.CompareTag("Unit"))
            {
                MoveAgent otherAgent = hit.collider.GetComponent<MoveAgent>();
                // ≈сли тот, кто впереди, уже стоит или почти у цели
                if (otherAgent != null && (otherAgent.IsCompleted ||
                    Vector3.Distance(otherAgent.transform.position, otherAgent.destination) <= 0.5f))
                {
                    return true; // путь зан€т, надо остановитьс€
                }
            }
        }

        return false;
    }

    #endregion

    #region Editor

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        try
        {
            this.DrawMovingPath();
        }
        catch (Exception)
        {

        }
    }

    private void DrawMovingPath()
    {
        Gizmos.color = Color.magenta;

        var current = this.transform.position;
        for (int i = this.pointer; i < this.pointPath.Length; i++)
        {
            Gizmos.DrawLine(current, this.pointPath[i]);
            current = this.pointPath[i];
        }
    }

#endif

    #endregion

}
