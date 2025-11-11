using UnityEngine;
using System.Collections.Generic;
using Zenject;
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
        _gameActions.Cancel.started += OnCancel;
        _gameActions.Confirm.started += OnConfirm;
        _gameActions.Select.started += OnSelect;
        _gameActions.Enable();
    }

    private void OnDestroy()
    {
        if (_cellManager != null)
            _cellManager.OnCellClicked.RemoveListener(HandleCellClick);

        if (_gameActions.enabled)
        {
            _gameActions.Cancel.started -= OnCancel;
            _gameActions.Confirm.started -= OnConfirm;
            _gameActions.Select.started -= OnSelect;
            _gameActions.Disable();
        }
    }

    private void OnCancel(InputAction.CallbackContext context)
    {
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


    private void HighlightSelectedUnit(Unit unit)
    {
        Vector3 currentPos = unit.transform.position;
        unit.transform.position = new Vector3(currentPos.x, currentPos.y + 0.5f, currentPos.z);
    }
    private void UnhighlightSelectedUnit(Unit unit)
    {
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
            var jumpMoves = GetKingJumpMoves(unit);

            Debug.Log($"King has {kingMoves.Count} total moves, {jumpMoves.Count} jumps");

            // Прыжки - красный
            foreach (var cell in jumpMoves)
            {
                cell.SetSelect(_palette.AttackCell);
            }

            // Обычные ходы - зеленый (только если нет прыжков)
            if (jumpMoves.Count == 0)
            {
                foreach (var cell in kingMoves)
                {
                    cell.SetSelect(_palette.MoveCell);
                }
            }

            return;
        }

        // === ИСПРАВЛЕНИЕ ДЛЯ ОБЫЧНЫХ ШАШЕК ===

        // Сначала ищем прыжки
        List<Cell> jumpMovesNormal = new List<Cell>();
        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                jumpMovesNormal.Add(cell);
            }
        }

        // Если есть прыжки - подсвечиваем ТОЛЬКО их красным
        if (jumpMovesNormal.Count > 0)
        {
            foreach (var cell in jumpMovesNormal)
            {
                cell.SetSelect(_palette.AttackCell);
                Debug.Log($"Regular piece JUMP to: {cell.transform.position}");
            }
            return;
        }

        // Если прыжков нет - подсвечиваем обычные ходы зеленым
        foreach (var cell in allCells)
        {
            if (cell.Unit == null && IsDiagonalForward(unit, cell))
            {
                float dist = Vector3.Distance(unit.Cell.transform.position, cell.transform.position);
                if (Mathf.Abs(dist - Mathf.Sqrt(8)) < 0.1f)
                {
                    cell.SetSelect(_palette.MoveCell);
                    Debug.Log($" Regular piece move to: {cell.transform.position}");
                }
            }
        }
    }

    //private void HighlightJumpMoves(Unit unit)
    //{
    //    var jumps = GetJumpMoves(unit);
    //    foreach (var cell in jumps)
    //    {
    //        cell.SetSelect(_palette.AttackCell);
    //    }
    //}

    private void MoveUnit(Unit unit, Cell targetCell)
    {
        bool wasJump = false;
        Cell enemyCell = null;

        if (unit.IsKing)
        {
            wasJump = CanKingJumpOver(unit, targetCell, out enemyCell);
        }
        else
        {
            wasJump = CanJumpOver(unit, targetCell, out enemyCell);
        }

        if (wasJump && enemyCell != null)
        {
            Destroy(enemyCell.Unit.gameObject);
            enemyCell.Unit = null;
        }

        // Перемещение
        unit.Cell.Unit = null;
        targetCell.Unit = unit;
        unit.Cell = targetCell;
        unit.transform.position = targetCell.transform.position + Vector3.up * 1f;

        // Проверка на дамку
        if (!unit.IsKing && IsOnOppositeEdge(unit))
        {
            unit.IsKing = true;
            HighlightAsKing(unit);
        }

        // ЛОГИКА СЕРИИ АТАК (ОБНОВЛЕННАЯ)
        if (wasJump)
        {
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

                foreach (var cell in nextJumps)
                {
                    cell.SetSelect(_palette.AttackCell);
                }
                return; // НЕ переключаем ход, игрок продолжает прыжки
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

        if (targetCell.Unit != null) return false;

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;
        Vector3 jumpDir = to - from;

        if (Mathf.Abs(jumpDir.magnitude - Mathf.Sqrt(32)) > 0.1f) return false;

        Vector3 stepDir = new Vector3(
            Mathf.Sign(jumpDir.x) * 2f,
            0,
            Mathf.Sign(jumpDir.z) * 2f
        );

        Vector3 enemyPos = from + stepDir;

        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);
        foreach (var cell in allCells)
        {
            if (Vector3.Distance(cell.transform.position, enemyPos) < 0.1f)
            {
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

        if (unit.Team == Team.Player1)
        {
            // Player1 (серые) стартуют СЛЕВА (столбцы 0,1,2)
            // Противоположный край для них - ПРАВАЯ СТОРОНА (столбец 7)
            bool isOpposite = x == 7;
            return isOpposite;
        }
        else // Team.Player2
        {
            // Player2 (синие) стартуют СПРАВА (столбцы 5,6,7)
            // Противоположный край для них - ЛЕВАЯ СТОРОНА (столбец 0)
            bool isOpposite = x == 0;
            return isOpposite;
        }
    }


    private void HighlightAsKing(Unit unit)
    {
        unit.GetComponent<MeshRenderer>().material = KingMaterial;
    }

    private List<Cell> GetKingMoves(Unit unit)
    {
        List<Cell> moves = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        var jumpMoves = GetKingJumpMoves(unit);
        if (jumpMoves.Count > 0)
        {
            return jumpMoves;
        }

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

        return moves;
    }


    private bool IsPathClear(Cell from, Cell to)
    {
        Vector3 fromPos = from.transform.position;
        Vector3 toPos = to.transform.position;
        Vector3 dir = (toPos - fromPos).normalized;

        float distance = Vector3.Distance(fromPos, toPos);

        int steps = Mathf.RoundToInt(distance / 2f);

        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        for (int i = 1; i < steps; i++)
        {
            Vector3 checkPos = fromPos + dir * (i * 2f);

            foreach (var cell in allCells)
            {
                if (Vector3.Distance(cell.transform.position, checkPos) < 0.1f)
                {
                    if (cell.Unit != null)
                    {
                        return false;
                    }
                    break; 
                }
            }
        }
        return true;
    }

    private List<Cell> GetKingJumpMoves(Unit unit)
    {
        List<Cell> jumps = new List<Cell>();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        foreach (var cell in allCells)
        {
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
            return false;
        }

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;

        float dx = Mathf.Abs(to.x - from.x);
        float dz = Mathf.Abs(to.z - from.z);

        if (Mathf.Abs(dx - dz) > 0.1f)
        {
            return false;
        }

        Vector3 dir = new Vector3(
            to.x > from.x ? 1 : -1,
            0,
            to.z > from.z ? 1 : -1
        );

        int steps = Mathf.RoundToInt(dx / 2f);

        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);
        Cell foundEnemy = null;


        for (int i = 1; i <= steps; i++)
        {
            Vector3 checkPos = new Vector3(
                from.x + dir.x * (i * 2f),
                from.y,
                from.z + dir.z * (i * 2f)
            );

            Cell cell = FindExactCellAtPosition(checkPos, allCells);

            if (cell == null)
            {
                continue;
            }

            if (cell.Unit != null)
            {
                if (cell.Unit.Team == unit.Team)
                {
                    return false;
                }

                if (foundEnemy != null)
                {
                    return false;
                }
                foundEnemy = cell;
            }
        }

        if (foundEnemy == null)
        {
            return false;
        }

        Vector3 enemyPos = foundEnemy.transform.position;
        Vector3 expectedLanding = new Vector3(
            enemyPos.x + dir.x * 2f,
            enemyPos.y,
            enemyPos.z + dir.z * 2f
        );

        if (Vector3.Distance(targetCell.transform.position, expectedLanding) > 0.1f)
        {
            return false;
        }

        enemyCell = foundEnemy;
        return true;
    }


    private Cell FindExactCellAtPosition(Vector3 position, Cell[] cells)
    {
        foreach (var cell in cells)
        {
            if (Mathf.Abs(cell.transform.position.x - position.x) < 0.1f &&
                Mathf.Abs(cell.transform.position.z - position.z) < 0.1f)
            {
                return cell;
            }
        }
        return null;
    }

}