using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Game.GameEngine.Ecs;
using SampleProject.ResourceObject;
using SampleProject.Base;

public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private RectTransform selectionBox;
    [SerializeField] private Transform[] patrolPoints;

    private List<Entity> selectedUnits = new List<Entity>();
    private Vector2 mouseStartPosition;
    private bool isSelecting;

    private Color selectedColor = Color.green;

    // Словарь для хранения оригинальных цветов юнитов
    private Dictionary<int, Color> originalColors = new Dictionary<int, Color>(); // Используем ID вместо ссылки

    private void Update()
    {
        HandleSelection();
        HandleCommand();

        if (Input.GetKeyDown(KeyCode.P) && selectedUnits.Count > 0)
        {
            CommandPatrol();
        }

        // Очищаем словарь от мёртвых ссылок (опционально)
        CleanupDeadReferences();
    }

    private void CleanupDeadReferences()
    {
        // Проверяем и удаляем записи для уничтоженных юнитов
        var deadKeys = originalColors.Keys.Where(id =>
        {
            // Проверяем, существует ли ещё Entity с таким ID
            var entities = FindObjectsOfType<Entity>();
            return !entities.Any(e => e.IsExists() && e.GetHashCode() == id);
        }).ToList();

        foreach (var key in deadKeys)
        {
            originalColors.Remove(key);
        }
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseStartPosition = Input.mousePosition;
            isSelecting = true;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(true);
                selectionBox.position = mouseStartPosition;
                selectionBox.sizeDelta = Vector2.zero;
            }
        }

        if (Input.GetMouseButton(0) && isSelecting)
        {
            if (selectionBox != null)
            {
                Vector2 currentMousePos = Input.mousePosition;
                Vector2 boxStart = mouseStartPosition;
                Vector2 boxEnd = currentMousePos;

                float width = boxEnd.x - boxStart.x;
                float height = boxEnd.y - boxStart.y;

                selectionBox.sizeDelta = new Vector2(Mathf.Abs(width), Mathf.Abs(height));
                selectionBox.position = boxStart + new Vector2(width / 2, height / 2);
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;

            if (selectionBox != null)
            {
                selectionBox.gameObject.SetActive(false);
            }

            FinishSelection();
        }
    }

    private void FinishSelection()
    {
        Vector2 dragSize = (Vector2)Input.mousePosition - mouseStartPosition;

        if (dragSize.magnitude < 10f)
        {
            SelectSingleUnit();
        }
        else
        {
            SelectUnitsInRect();
        }
    }

    private void SelectSingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            if (hit.collider.CompareTag("Unit"))
            {
                Entity entity = hit.collider.GetComponent<Entity>();
                if (entity != null && entity.IsExists())
                {
                    ClearSelection();
                    AddToSelection(entity);
                    Debug.Log($"Selected single unit: {entity.name}");
                }
            }
            else
            {
                ClearSelection();
            }
        }
        else
        {
            ClearSelection();
        }
    }

    private void SelectUnitsInRect()
    {
        ClearSelection();

        float minX = Mathf.Min(mouseStartPosition.x, Input.mousePosition.x);
        float maxX = Mathf.Max(mouseStartPosition.x, Input.mousePosition.x);
        float minY = Mathf.Min(mouseStartPosition.y, Input.mousePosition.y);
        float maxY = Mathf.Max(mouseStartPosition.y, Input.mousePosition.y);

        GameObject[] allUnits = GameObject.FindGameObjectsWithTag("Unit");
        List<Entity> unitsInRect = new List<Entity>();

        foreach (var unitObj in allUnits)
        {
            if (unitObj == null) continue;

            Entity entity = unitObj.GetComponent<Entity>();
            if (entity == null || !entity.IsExists()) continue;

            Vector3 screenPos = mainCamera.WorldToScreenPoint(entity.transform.position);

            if (screenPos.z > 0 &&
                screenPos.x >= minX && screenPos.x <= maxX &&
                screenPos.y >= minY && screenPos.y <= maxY)
            {
                unitsInRect.Add(entity);
            }
        }

        Debug.Log($"Found {unitsInRect.Count} units in rectangle");

        foreach (var unit in unitsInRect)
        {
            AddToSelection(unit);
        }

        Debug.Log($"Selected {selectedUnits.Count} units");
    }

    private void HandleCommand()
    {
        if (selectedUnits.Count == 0) return;

        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Entity targetEntity = hit.collider.GetComponent<Entity>();

                if (targetEntity != null && targetEntity is ResourceEntity)
                {
                    CommandGatherResource(targetEntity);
                }
                else if (hit.collider.CompareTag("Enemy"))
                {
                    CommandAttackTarget(targetEntity);
                }
                else if (targetEntity != null && targetEntity.HasData<TeamComponent>() == false)
                {
                    CommandAttackTarget(targetEntity);
                }
                else if (hit.transform.CompareTag("Ground"))
                {
                    CommandMoveToPosition(hit.point);
                }
            }
        }
    }

    private void CommandMoveToPosition(Vector3 position)
    {
        int groupId = System.Guid.NewGuid().GetHashCode();

        for (int i = 0; i < selectedUnits.Count; i++)
        {
            var unit = selectedUnits[i];
            if (unit != null && unit.IsExists())
            {
                bool isLeader = (i == 0);
                float waitDistance = isLeader ? 0.5f : 1.0f + (i * 0.15f);

                unit.SetData(new GroupMoveData
                {
                    destination = position,
                    isGroupLeader = isLeader,
                    groupId = groupId,
                    waitDistance = waitDistance,
                    hasStopped = false
                });

                unit.SetData(new CommandRequest
                {
                    type = CommandType.MOVE_TO_POSITION,
                    args = position,
                    status = CommandStatus.IDLE
                });
            }
        }

        Debug.Log($"Moving {selectedUnits.Count} units to {position} (Group ID: {groupId})");
    }

    private void CommandAttackTarget(Entity target)
    {
        Debug.Log($"{selectedUnits.Count} units attacking {target.name}");

        foreach (var unit in selectedUnits)
        {
            if (unit != null && unit.IsExists())
            {
                unit.SetData(new CommandRequest
                {
                    type = CommandType.ATTACK_TARGET,
                    args = target,
                    status = CommandStatus.IDLE
                });
            }
        }
    }

    private void CommandGatherResource(Entity resource)
    {
        Debug.Log($"{selectedUnits.Count} units gathering from {resource.name}");

        foreach (var unit in selectedUnits)
        {
            if (unit != null && unit.IsExists())
            {
                unit.SetData(new CommandRequest
                {
                    type = CommandType.GATHER_RESOURCE,
                    args = resource,
                    status = CommandStatus.IDLE
                });
            }
        }
    }

    private void CommandPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("No patrol points assigned!");
            return;
        }

        List<Vector3> pointsPositions = new List<Vector3>();
        foreach (var point in patrolPoints)
        {
            if (point != null)
                pointsPositions.Add(point.position);
        }

        Debug.Log($"Starting patrol for {selectedUnits.Count} units on {pointsPositions.Count} points");

        foreach (var unit in selectedUnits)
        {
            if (unit != null && unit.IsExists())
            {
                unit.SetData(new CommandRequest
                {
                    type = CommandType.PATROL_BY_POINTS,
                    args = pointsPositions,
                    status = CommandStatus.IDLE
                });
            }
        }

        if (pointsPositions.Count > 0)
        {
            int groupId = System.Guid.NewGuid().GetHashCode();
            for (int i = 0; i < selectedUnits.Count; i++)
            {
                var unit = selectedUnits[i];
                if (unit != null && unit.IsExists())
                {
                    bool isLeader = (i == 0);
                    float waitDistance = isLeader ? 0.5f : 1.0f + (i * 0.15f);

                    unit.SetData(new GroupMoveData
                    {
                        destination = pointsPositions[0],
                        isGroupLeader = isLeader,
                        groupId = groupId,
                        waitDistance = waitDistance,
                        hasStopped = false
                    });
                }
            }
        }
    }

    private void AddToSelection(Entity entity)
    {
        if (entity == null) return;
        if (!entity.IsExists()) return;
        if (selectedUnits.Contains(entity)) return;

        selectedUnits.Add(entity);
        SetUnitColor(entity, selectedColor);
    }

    private void ClearSelection()
    {
        foreach (var unit in selectedUnits)
        {
            if (unit != null && unit.IsExists())
            {
                RestoreOriginalColor(unit);
            }
        }
        selectedUnits.Clear();
    }

    private void SetUnitColor(Entity entity, Color color)
    {
        var renderer = GetRenderer(entity);
        if (renderer != null)
        {
            int entityId = entity.GetHashCode();

            // Сохраняем оригинальный цвет, если ещё не сохранён
            if (!originalColors.ContainsKey(entityId))
            {
                originalColors[entityId] = renderer.material.color;
            }

            renderer.material.color = color;
        }
    }

    private void RestoreOriginalColor(Entity entity)
    {
        var renderer = GetRenderer(entity);
        if (renderer != null)
        {
            int entityId = entity.GetHashCode();
            if (originalColors.ContainsKey(entityId))
            {
                renderer.material.color = originalColors[entityId];
                originalColors.Remove(entityId);
            }
        }
    }

    private Renderer GetRenderer(Entity entity)
    {
        if (entity == null) return null;

        var renderer = entity.GetComponent<Renderer>();
        if (renderer != null) return renderer;

        return entity.GetComponentInChildren<Renderer>();
    }
}