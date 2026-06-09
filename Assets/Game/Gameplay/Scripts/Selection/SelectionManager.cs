using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Game.GameEngine.Ecs;
using SampleProject.ResourceObject;
public class SelectionManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private LayerMask unitLayerMask;

    private List<Entity> selectedUnits = new List<Entity>();
    private Vector3 selectionStartPosition;
    private bool isSelecting;

    private void Update()
    {
        HandleSelection();
        HandleCommand();
    }

    private void HandleSelection()
    {
        if (Input.GetMouseButtonDown(0))
        {
            selectionStartPosition = GetMouseWorldPosition();
            isSelecting = true;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isSelecting = false;
            FinishSelection();
        }
    }

    private void FinishSelection()
    {
        Vector3 endPosition = GetMouseWorldPosition();
        Bounds selectionBounds = new Bounds(selectionStartPosition, Vector3.zero);
        selectionBounds.Encapsulate(endPosition);

        // Если не было драга - одиночный клик
        if (selectionBounds.size.magnitude < 0.1f)
        {
            SelectSingleUnit();
        }
        else
        {
            SelectUnitsInRect(selectionBounds);
        }
    }

    private void SelectSingleUnit()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, unitLayerMask))
        {
            Entity entity = hit.collider.GetComponent<Entity>();
            if (entity != null)
            {
                ClearSelection();
                AddToSelection(entity);
            }
        }
    }

    private void SelectUnitsInRect(Bounds bounds)
    {
        ClearSelection();

        Collider[] colliders = Physics.OverlapBox(bounds.center, bounds.extents, Quaternion.identity, unitLayerMask);
        foreach (var collider in colliders)
        {
            Entity entity = collider.GetComponent<Entity>();
            if (entity != null)
            {
                AddToSelection(entity);
            }
        }
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

                // СНАЧАЛА ПРОВЕРЯЕМ НА РЕСУРС
                if (targetEntity != null && targetEntity is ResourceEntity)
                {
                    CommandGatherResource(targetEntity);
                }
                // ПОТОМ НА ВРАГА
                else if (targetEntity != null && targetEntity.HasData<TeamComponent>() == false)
                {
                    CommandAttackTarget(targetEntity);
                }
                // ПОТОМ НА ЗЕМЛЮ
                else if (hit.transform.CompareTag("Ground"))
                {
                    CommandMoveToPosition(hit.point);
                }
            }
        }
    }

    private void CommandMoveToPosition(Vector3 position)
    {
        foreach (var unit in selectedUnits)
        {
            if (unit.IsExists())
            {
                unit.SetData(new CommandRequest
                {
                    type = CommandType.MOVE_TO_POSITION,
                    args = position,
                    status = CommandStatus.IDLE
                });
            }
        }
    }

    private void CommandAttackTarget(Entity target)
    {
        foreach (var unit in selectedUnits)
        {
            if (unit.IsExists())
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
        foreach (var unit in selectedUnits)
        {
            if (unit.IsExists())
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

    private void AddToSelection(Entity entity)
    {
        selectedUnits.Add(entity);
        // TODO: Добавить визуал выделения (OutLine или кольцо)
    }

    private void ClearSelection()
    {
        // TODO: Убрать визуал выделения
        selectedUnits.Clear();
    }

    private Vector3 GetMouseWorldPosition()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.point;
        }
        return Vector3.zero;
    }
}
