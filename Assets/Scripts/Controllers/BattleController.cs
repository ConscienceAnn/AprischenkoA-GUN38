using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Unity.VisualScripting;
using System.ComponentModel;
using UnityEngine.InputSystem;

public class BattleController : MonoBehaviour
{
    [Inject] private CellManager _cellManager;
    [Inject] private CellPaletteSettings _palette;
    [Inject] private GameInput _gameInput;

    [SerializeField] private Material KingMaterial;

    private Team _currentTeam = Team.Player1; // Player1 ходит первым
    private Unit _selectedUnit = null;
    private GameInput.GameActions _gameActions;

   
    private void Start()
    {

        _cellManager.OnCellClicked.AddListener(HandleCellClick);
        InitializeInputSystem();

    }

    private void InitializeInputSystem()
    {
        _gameActions = _gameInput.Game;

        // ПОДПИСКА НА СОБЫТИЯ
        _gameActions.Cancel.started += OnCancel;
        _gameActions.Confirm.started += OnConfirm;
        _gameActions.Select.started += OnSelect;

        _gameActions.Enable();

        Debug.Log("GameInput system initialized successfully");
    }

    private void OnDestroy()
    {
        // ОТПИСКА ОТ СОБЫТИЙ
        if (_cellManager != null)
            _cellManager.OnCellClicked.RemoveListener(HandleCellClick);

        // Для GameActions проверяем не через null, а через IsValid()
        if (_gameActions.enabled)
        {
            _gameActions.Cancel.started -= OnCancel;
            _gameActions.Confirm.started -= OnConfirm;
            _gameActions.Select.started -= OnSelect;
            _gameActions.Disable();
        }
    }

    // === INPUT HANDLERS ===
    private void OnCancel(InputAction.CallbackContext context)
    {
        Debug.Log("Cancel pressed (ESC)");
        ResetAllSelection();
    }

    private void OnConfirm(InputAction.CallbackContext context)
    {
        Debug.Log("Confirm pressed (Space)");
        // Подтверждение хода не реализовано, так как в шашках ход совершается мгновенно.
        // Кнопка Space зарезервирована под будущие механики (если потребуется).
    }

    private void OnSelect(InputAction.CallbackContext context)
    {
        Debug.Log("Select pressed (Left Mouse Button)");
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Cell cell = hit.collider.GetComponent<Cell>();
            if (cell != null)
            {
                // Создаём команду и выполняем её
                var command = new SelectCellCommand(this);
                command.Interact(cell);
            }
        }
    }


    public void HandleCellClick(Cell cell)
    {
        if (_selectedUnit == null)
        {
            // Обычный выбор шашки
            if (cell.Unit != null && cell.Unit.Team == _currentTeam)
            {
                ResetAllSelection();
                _selectedUnit = cell.Unit;
                HighlightSelectedUnit(_selectedUnit);
                HighlightPossibleMoves(_selectedUnit);
            }
        }
        else
        {
            // Получаем возможные ходы (обычные или только прыжки)
            List<Cell> possibleMoves = GetPossibleMoves(_selectedUnit);

            if (possibleMoves.Contains(cell))
            {
                MoveUnit(_selectedUnit, cell);
                // MoveUnit сам решит: продолжать ход или передавать
            }
            else if (cell.Unit != null && cell.Unit.Team == _currentTeam)
            {
                // Разрешаем сменить выбор ТОЛЬКО если нет активного прыжка
                // (но по правилам это не нужно — можно упростить)
                ResetAllSelection();
                _selectedUnit = cell.Unit;
                HighlightSelectedUnit(_selectedUnit);
                HighlightPossibleMoves(_selectedUnit);
            }
            else
            {
                ResetAllSelection();
            }
        }
    }

    // === НОВЫЕ МЕТОДЫ ===

    private void HighlightSelectedUnit(Unit unit)
    {
        // Просто поднимаем выше, но запоминаем только Y-компонент
        Vector3 currentPos = unit.transform.position;
        unit.transform.position = new Vector3(currentPos.x, currentPos.y + 0.5f, currentPos.z);
    }
    private void UnhighlightSelectedUnit(Unit unit)
    {

        // Возвращаем на базовую высоту над клеткой
        Vector3 currentPos = unit.transform.position;
        float baseHeight = unit.Cell.transform.position.y + 1f;
        unit.transform.position = new Vector3(currentPos.x, baseHeight, currentPos.z);
    }

    private void ClearHighlights()
    {
        var cells = FindObjectsByType<Cell>(FindObjectsSortMode.None);
        foreach (var cell in cells)
        {
            cell.ResetSelect();
        }
    }

    private List<Cell> GetPossibleMoves(Unit unit)
    {

        if (unit.IsKing)
            return GetKingMoves(unit);

        List<Cell> moves = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        // Сначала ищем прыжки
        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                moves.Add(cell);
            }
        }

        // Если есть прыжки — возвращаем только их
        if (moves.Count > 0)
            return moves;

        // Иначе — обычные ходы
        foreach (var cell in allCells)
        {
            if (cell.Unit == null && IsDiagonalForward(unit, cell))
            {
                float dist = Vector3.Distance(unit.Cell.transform.position, cell.transform.position);
                if (Mathf.Abs(dist - Mathf.Sqrt(8)) < 0.1f)
                {
                    moves.Add(cell);
                }
            }
        }

        return moves;
    }

    private bool IsDiagonalForward(Unit unit, Cell targetCell)
    {
        if (unit.IsKing)
            return IsDiagonalAny(unit, targetCell);

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;
        float dx = to.x - from.x;
        float dz = to.z - from.z;

        if (Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dz)) > 0.1f) return false;
        if (Mathf.Abs(dx) < 0.1f) return false;

        if (unit.Team == Team.Player1)
        {
            // Player1 (слева) движется ВПРАВО (увеличивать X)
            return dx > 0;
        }
        else
        {
            // Player2 (справа) движется ВЛЕВО (уменьшать X)
            return dx < 0;
        }
    }

    private bool IsDiagonalAny(Unit unit, Cell targetCell)
    {
        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;
        float dx = to.x - from.x;
        float dz = to.z - from.z;

        // Проверяем что это диагональ 
        bool isDiagonal = Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dz)) < 0.1f;

        // И что это не та же клетка
        bool notSameCell = Mathf.Abs(dx) > 0.1f;

        Debug.Log($"Diagonal check: x={dx}, z={dz}, isDiagonal={isDiagonal}, notSameCell={notSameCell}");

        return isDiagonal && notSameCell;
    }


    private void HighlightPossibleMoves(Unit unit)
    {
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        if (unit.IsKing)
        {
            Debug.Log($"Highlighting moves for KING at {unit.Cell.transform.position}");
            var kingMoves = GetKingMoves(unit);
            foreach (var cell in kingMoves)
            {
                cell.SetSelect(_palette.MoveCell);
                Debug.Log($"King can move to: {cell.transform.position}");
            }
            return;
        }

        // Сначала ищем прыжки
        List<Cell> jumpMoves = new();
        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                jumpMoves.Add(cell);
            }
        }

        // Если есть прыжки — подсвечиваем ТОЛЬКО их как AttackCell
        if (jumpMoves.Count > 0)
        {
            foreach (var cell in jumpMoves)
            {
                cell.SetSelect(_palette.AttackCell);
            }
            return;
        }

        // Иначе — обычные ходы как MoveCell
        foreach (var cell in allCells)
        {
            if (cell.Unit == null && IsDiagonalForward(unit, cell))
            {
                float dist = Vector3.Distance(unit.Cell.transform.position, cell.transform.position);
                if (Mathf.Abs(dist - Mathf.Sqrt(8)) < 0.1f)
                {
                    cell.SetSelect(_palette.MoveCell);
                }
            }
        }
    }

    private void HighlightJumpMoves(Unit unit)
    {
        var jumps = GetJumpMoves(unit);
        foreach (var cell in jumps)
        {
            cell.SetSelect(_palette.AttackCell);
        }
    }

    private void MoveUnit(Unit unit, Cell targetCell)
    {
        bool wasJump = false;
        Cell enemyCell = null;

        // Разделяем логику проверки прыжков
        if (unit.IsKing)
        {
            wasJump = CanKingJumpOver(unit, targetCell, out enemyCell);
            Debug.Log($"King jump check: wasJump={wasJump}, enemyCell={enemyCell != null}");
        }
        else
        {
            wasJump = CanJumpOver(unit, targetCell, out enemyCell);
        }

        if (wasJump && enemyCell != null)
        {
            Debug.Log($"DESTROYING enemy unit at {enemyCell.transform.position}");
            Destroy(enemyCell.Unit.gameObject);
            enemyCell.Unit = null;
            wasJump = true;
        }
        else if (wasJump && enemyCell == null)
        {
            Debug.LogError("JUMP WAS DETECTED BUT NO ENEMY CELL FOUND!");
            wasJump = false; // Отменяем прыжок если нет вражеской клетки
        }


        // Перемещение
        unit.Cell.Unit = null;
        targetCell.Unit = unit;
        unit.Cell = targetCell;
        unit.transform.position = targetCell.transform.position + Vector3.up * 1f;

        Vector2Int coords = _cellManager.WorldToBoardCoords(targetCell.transform.position);
        Debug.Log($"Unit moved to position: {targetCell.transform.position} -> board coords: ({coords.x}, {coords.y})");

        // Проверка на дамку
        if (!unit.IsKing && IsOnOppositeEdge(unit))
        {
            unit.IsKing = true;
            HighlightAsKing(unit);
        }

        if (wasJump)
        {
            // РАЗДЕЛЯЕМ ЛОГИКУ ДЛЯ ПОСЛЕДУЮЩИХ ПРЫЖКОВ
            List<Cell> nextJumps;
            if (unit.IsKing)
            {
                nextJumps = GetKingJumpMoves(unit);
            }
            else
            {
                nextJumps = GetJumpMoves(unit);
            }

            if (nextJumps.Count > 0)
            {
                _selectedUnit = unit;
                HighlightSelectedUnit(unit);
                ClearHighlights();


                HighlightJumpMoves(unit);
                return;
            }
        }

        ResetAllSelection();
        SwitchTurn();
    }

    private void SwitchTurn()
    {
        _currentTeam = _currentTeam == Team.Player1 ? Team.Player2 : Team.Player1;
    }

    private void ResetAllSelection()
    {
        // Снимаем выделение с текущей шашки
        if (_selectedUnit != null)
        {
            UnhighlightSelectedUnit(_selectedUnit);
            _selectedUnit = null;
        }

        // Очищаем подсветки ходов
        ClearHighlights();
    }


    private bool CanJumpOver(Unit unit, Cell targetCell, out Cell enemyCell)
    {
        enemyCell = null;

        // Целевая клетка должна быть пустой
        if (targetCell.Unit != null) return false;

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;

        // Вектор прыжка
        Vector3 jumpDir = to - from;

        // Прыжок должен быть на 2 клетки по диагонали
        if (Mathf.Abs(jumpDir.magnitude - Mathf.Sqrt(32)) > 0.1f) return false;

        // Направление на одну клетку
        Vector3 stepDir = new Vector3(
            Mathf.Sign(jumpDir.x) * 2f,
            0,
            Mathf.Sign(jumpDir.z) * 2f
        );

        // Позиция вражеской клетки (между from и to)
        Vector3 enemyPos = from + stepDir;

        // Находим вражескую клетку
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            if (Vector3.Distance(cell.transform.position, enemyPos) < 0.1f)
            {
                // Это должна быть вражеская шашка
                if (cell.Unit != null && cell.Unit.Team != unit.Team)
                {
                    enemyCell = cell;
                    return true;
                }
            }
        }

        return false;
    }


    private List<Cell> GetJumpMoves(Unit unit)
    {
        List<Cell> jumps = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                jumps.Add(cell);
            }
        }

        return jumps;
    }

    private bool IsOnOppositeEdge(Unit unit)
    {
        Vector3 worldPos = unit.Cell.transform.position;

        Vector2Int coords = _cellManager.WorldToBoardCoords(worldPos);
        int x = coords.x;

        Debug.Log($"Unit world position: ({worldPos.x}, {worldPos.y}, {worldPos.z})");
        Debug.Log($"Unit at column {x}, team: {unit.Team}");

        if (unit.Team == Team.Player1)
        {
            // Player1 (серые) стартуют СЛЕВА (столбцы 0,1,2)
            // Противоположный край для них - ПРАВАЯ СТОРОНА (столбцы 5,6,7)
            bool isOpposite = x == 7;
            Debug.Log($"Player1 opposite edge check: column {x} >= 7 = {isOpposite}");
            return isOpposite;
        }
        else // Team.Player2
        {
            // Player2 (синие) стартуют СПРАВА (столбцы 5,6,7)
            // Противоположный край для них - ЛЕВАЯ СТОРОНА (столбцы 0,1,2)
            bool isOpposite = x == 0;
            Debug.Log($"Player2 opposite edge check: column {x} <= 0 = {isOpposite}");
            return isOpposite;
        }
    }


    private void HighlightAsKing(Unit unit)
    {


        // Или изменим цвет (если используешь material):
        unit.GetComponent<MeshRenderer>().material = KingMaterial;
    }

    private List<Cell> GetKingMoves(Unit unit)
    {
        List<Cell> moves = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        // Сначала ищем прыжки для дамки
        var jumpMoves = GetKingJumpMoves(unit);
        if (jumpMoves.Count > 0)
        {
            Debug.Log($"King has {jumpMoves.Count} jump moves - MUST JUMP");
            return jumpMoves; // Возвращаем ТОЛЬКО прыжки
        }

        // Иначе — обычные ходы дамки
        foreach (var cell in allCells)
        {
            if (cell.Unit == null && IsDiagonalAny(unit, cell))
            {
                if (IsPathClear(unit.Cell, cell))
                {
                    moves.Add(cell);
                }
            }
        }

        Debug.Log($"King has {moves.Count} regular moves");
        return moves;
    }


    private bool IsPathClear(Cell from, Cell to)
    {
        Vector3 fromPos = from.transform.position;
        Vector3 toPos = to.transform.position;

        // Определяем направление
        Vector3 dir = (toPos - fromPos).normalized;

        // Расстояние между клетками
        float distance = Vector3.Distance(fromPos, toPos);

        // Количество промежуточных клеток (шаг = 2 единицы)
        int steps = Mathf.RoundToInt(distance / 2f);

        Debug.Log($"Path check: from {fromPos} to {toPos}, distance: {distance}, steps: {steps}");

        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        for (int i = 1; i < steps; i++)
        {
            Vector3 checkPos = fromPos + dir * (i * 2f);

            // Ищем клетку в этой позиции
            foreach (var cell in allCells)
            {
                if (Vector3.Distance(cell.transform.position, checkPos) < 0.1f)
                {
                    if (cell.Unit != null)
                    {
                        Debug.Log($"Path blocked at step {i}, position {checkPos} by unit at {cell.transform.position}");
                        return false;
                    }
                    break; // нашли клетку, переходим к следующему шагу
                }
            }
        }

        Debug.Log("Path is clear!");
        return true;
    }

    private List<Cell> GetKingJumpMoves(Unit unit)
    {
        List<Cell> jumps = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        foreach (var cell in allCells)
        {
            // ИСПОЛЬЗУЕМ НОВЫЙ МЕТОД вместо CanJumpOver
            if (CanKingJumpOver(unit, cell, out _))
            {
                jumps.Add(cell);
            }
        }

        return jumps;
    }


    private bool CanKingJumpOver(Unit unit, Cell targetCell, out Cell enemyCell)
    {
        enemyCell = null;
        if (targetCell.Unit != null)
        {
            Debug.Log("King jump failed: target cell occupied");
            return false;
        }

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;

        // Проверяем что это диагональ
        if (!IsDiagonalAny(unit, targetCell))
        {
            Debug.Log("King jump failed: not diagonal");
            return false;
        }

        Vector3 dir = (to - from).normalized;
        float distance = Vector3.Distance(from, to);
        int steps = Mathf.RoundToInt(distance / 2f);

        bool foundEnemy = false;
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        Debug.Log($"King jump checking {steps - 1} intermediate cells");

        for (int i = 1; i < steps; i++)
        {
            Vector3 checkPos = from + dir * (i * 2f);

            foreach (var cell in allCells)
            {
                if (Vector3.Distance(cell.transform.position, checkPos) < 0.1f)
                {
                    if (cell.Unit != null)
                    {
                        if (cell.Unit.Team == unit.Team)
                        {
                            Debug.Log($"King jump blocked by own unit at {checkPos}");
                            return false;
                        }
                        else if (!foundEnemy)
                        {
                            enemyCell = cell;
                            foundEnemy = true;
                            Debug.Log($"Found enemy unit at {checkPos}, team: {cell.Unit.Team}");
                        }
                        else
                        {
                            Debug.Log($"King jump blocked by second enemy unit at {checkPos}");
                            return false;
                        }
                    }
                    break;
                }
            }
        }

        if (foundEnemy)
        {
            // Позиция за вражеской шашкой
            Vector3 afterEnemyPos = enemyCell.transform.position + dir * 2f;

            foreach (var cell in allCells)
            {
                if (Vector3.Distance(cell.transform.position, afterEnemyPos) < 0.1f)
                {
                    if (cell.Unit == null)
                    {
                        Debug.Log($"King jump successful: free cell found at {afterEnemyPos}");
                        return true;
                    }
                    else
                    {
                        Debug.Log($"King jump failed: cell after enemy is occupied at {afterEnemyPos}");
                        return false;
                    }
                }
            }
        }


        Debug.Log($"King jump final: foundEnemy={foundEnemy}, enemyCell={enemyCell != null}");
        return foundEnemy;
    }


}