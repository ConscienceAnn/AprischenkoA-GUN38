using System.Collections;
using UnityEngine;

public class AngryTowerFOV : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField, Range(0, 360)] private float _angle;

    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private Player _player;

    [SerializeField] private float checkDelay = 0.2f;

    [Header("Цвета визуализации")]
    [SerializeField] private Color radiusColor = Color.white;
    [SerializeField] private Color angleColor = Color.yellow;
    [SerializeField] private Color detectedColor = Color.red;
    [SerializeField] private Color normalColor = Color.green;
    [SerializeField] private Color sectorColor = new Color(1f, 1f, 0f, 0.3f);

    private bool _canSeePlayer;

    void Start()
    {
        StartCoroutine(FovRoutine());
    }

    private IEnumerator FovRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(checkDelay);

        while (_player != null)
        {
            yield return wait;
            FOVCheck();
        }
    }

    private void FOVCheck()
    {
        if (_player == null) return;

        // Проверка: игрок в радиусе?
        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        bool inRadius = distanceToPlayer <= _radius;

        // Если игрок вне радиуса - сразу выходим
        if (!inRadius)
        {
            _canSeePlayer = false;
            return;
        }

        // Теперь проверяем через OverlapCircle с учетом слоев
        Collider2D[] rangeChecks = Physics2D.OverlapCircleAll(transform.position, _radius, targetMask);

        bool foundPlayerInMask = false;
        foreach (var collider in rangeChecks)
        {
            if (collider.gameObject == _player.gameObject)
            {
                foundPlayerInMask = true;
                break;
            }
        }

        if (!foundPlayerInMask)
        {
            _canSeePlayer = false;
            return;
        }

        // Игрок в радиусе и в правильном слое - проверяем угол и препятствия
        Vector2 directionToPlayer = (_player.transform.position - transform.position).normalized;
        float angleToPlayer = Vector2.Angle(transform.up, directionToPlayer);
        bool inAngle = angleToPlayer <= _angle / 2;

        if (!inAngle)
        {
            _canSeePlayer = false;
            return;
        }

        // Проверяем препятствия
        RaycastHit2D hit = Physics2D.Raycast(transform.position, directionToPlayer, distanceToPlayer, obstacleMask);

        if (hit.collider == null)
        {
            _canSeePlayer = true;
        }
        else
        {
            _canSeePlayer = false;
        }
    }

    void OnDrawGizmos()
    {
        // 1. Рисуем радиус
        Gizmos.color = new Color(radiusColor.r, radiusColor.g, radiusColor.b, 0.1f);
        Gizmos.DrawSphere(transform.position, _radius);
        Gizmos.color = radiusColor;
        Gizmos.DrawWireSphere(transform.position, _radius);

        // 2. Рисуем сектор
        Gizmos.color = angleColor;
        Vector3 leftDirection = DirectionFromAngle2D(-_angle / 2);
        Vector3 rightDirection = DirectionFromAngle2D(_angle / 2);
        Gizmos.DrawRay(transform.position, leftDirection * _radius);
        Gizmos.DrawRay(transform.position, rightDirection * _radius);

        DrawSectorGizmo();

        // 3. Рисуем линию к игроку
        if (_player != null)
        {
            Gizmos.color = _canSeePlayer ? detectedColor : normalColor;
            Gizmos.DrawLine(transform.position, _player.transform.position);

            // Дополнительная визуализация препятствий
            if (Application.isPlaying && !_canSeePlayer)
            {
                Vector2 dir = (_player.transform.position - transform.position).normalized;
                float dist = Vector2.Distance(transform.position, _player.transform.position);
                RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, dist, obstacleMask);

                if (hit.collider != null)
                {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(transform.position, hit.point);
                    Gizmos.DrawSphere(hit.point, 0.2f);
                }
            }
        }
    }

    private void DrawSectorGizmo()
    {
        int segments = Mathf.RoundToInt(_angle);
        float stepAngleSize = _angle / segments;

        Vector3[] points = new Vector3[segments + 2];
        points[0] = transform.position;

        for (int i = 0; i <= segments; i++)
        {
            float angle = -_angle / 2 + stepAngleSize * i;
            Vector3 dir = DirectionFromAngle2D(angle);
            points[i + 1] = transform.position + dir * _radius;
        }

        // Рисуем залитый сектор
        Gizmos.color = sectorColor;
        for (int i = 1; i < points.Length - 1; i++)
        {
            Gizmos.DrawLine(points[0], points[i]);
            Gizmos.DrawLine(points[i], points[i + 1]);
        }

        // Замыкаем
        Gizmos.DrawLine(points[points.Length - 1], points[1]);
    }

    private Vector3 DirectionFromAngle2D(float angleInDegrees)
    {
        angleInDegrees += transform.eulerAngles.z - 90;

        return new Vector3(
            Mathf.Cos(angleInDegrees * Mathf.Deg2Rad),
            Mathf.Sin(angleInDegrees * Mathf.Deg2Rad),
            0
        );
    }
}