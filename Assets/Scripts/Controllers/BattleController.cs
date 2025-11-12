using UnityEngine;
using System.Collections.Generic;
using Zenject;
using UnityEngine.InputSystem;
using System.Linq;
using Unity.VisualScripting;

public class BattleController : MonoBehaviour
{
    [Inject] private CellManager _cellManager;
    [Inject] private CellPaletteSettings _palette;
    [Inject] private GameInput _gameInput;

    public event System.Action<Team> OnTurnChanged;

    [SerializeField] private Material KingMaterial;

    private Team _currentTeam = Team.Player1; // Player1 ходит первым
    private Unit _selectedUnit = null;
    private GameInput.GameActions _gameActions;

    private List<Unit> _unitsWithJumps = new List<Unit>();
    private bool _mustAttack = false;


    private void Start()
    {

        _cellManager.OnCellClicked.AddListener(HandleCellClick);
        InitializeInputSystem();
        CleanupDestroyedUnits();

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
        // Кнопка Space видимо зарезервирована например под будущие механики (если потребуется).
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
        Debug.Log($"=== HandleCellClick ===");
        Debug.Log($"Click on cell at {cell.transform.position}");

        Vector2Int cellCoords = _cellManager.WorldToBoardCoords(cell.transform.position);
        Debug.Log($"Координаты клетки: ({cellCoords.x},{cellCoords.y})");

        Debug.Log($"Состояние: _selectedUnit={_selectedUnit != null}, _mustAttack={_mustAttack}, _unitsWithJumps.Count={_unitsWithJumps.Count}");

        Debug.Log($"Шашки с прыжками ({_unitsWithJumps.Count}):");
        foreach (var unit in _unitsWithJumps)
        {
            if (IsUnitValid(unit))
            {
                Vector2Int coords = _cellManager.WorldToBoardCoords(unit.Cell.transform.position);
                Debug.Log($"  - {unit.Team} at ({coords.x},{coords.y})");
            }
            else
            {
                Debug.Log($"  - Уничтожённый юнит (удаляю из списка)");
            }
        }

        _unitsWithJumps.RemoveAll(u => !IsUnitValid(u));

       
        if (_selectedUnit == null)
        {
            if (cell.Unit != null && cell.Unit.Team == _currentTeam)
            {
                Debug.Log($"Клик на свою шашку: {cell.Unit.Team} at {cell.transform.position}");
                Vector2Int unitCoords = _cellManager.WorldToBoardCoords(cell.Unit.Cell.transform.position);
                Debug.Log($"Координаты шашки: ({unitCoords.x},{unitCoords.y})");

                if (_mustAttack)
                {
                    bool canAttack = _unitsWithJumps.Contains(cell.Unit);
                    Debug.Log($"Обязательная атака! Эта шашка может атаковать: {canAttack}");

                    if (!canAttack)
                    {
                        Debug.Log("ОШИБКА: Должны выбрать шашку, которая может атаковать!");
                        if (cell.Unit != null)
                            DebugUnitPosition(cell.Unit, "Попытка выбора");
                        else
                            Debug.Log("Юнит был уничтожен!");
                        return;
                    }
                }

                ResetAllSelection();
                _selectedUnit = cell.Unit;
                HighlightSelectedUnit(_selectedUnit);
                HighlightPossibleMoves(_selectedUnit);

                DebugUnitPosition(_selectedUnit, "Выбрана шашка");
            }
        }
        else
        {
            List<Cell> possibleMoves = GetPossibleMoves(_selectedUnit);

            if (possibleMoves.Contains(cell))
            {
                MoveUnit(_selectedUnit, cell);
            }
            else if (cell.Unit != null && cell.Unit.Team == _currentTeam)
            {
               
                if (!_mustAttack || _unitsWithJumps.Contains(cell.Unit))
                {
                    ResetAllSelection();
                    _selectedUnit = cell.Unit;
                    HighlightSelectedUnit(_selectedUnit);
                    HighlightPossibleMoves(_selectedUnit);
                }
                else
                {
                    Debug.Log("Нельзя перевыбрать: обязательная атака другой шашкой!");
                }
            }
            else
            {
                
                ResetAllSelection();
               
                if (_mustAttack)
                    PrepareTurn();
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
        if (!IsUnitValid(unit)) return new List<Cell>();

        if (unit.IsKing)
            return GetKingMoves(unit);

        List<Cell> moves = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                moves.Add(cell);
            }
        }

        if (moves.Count > 0)
            return moves;

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
            return dx > 0;
        }
        else
        {
            return dx < 0;
        }
    }

    private bool IsDiagonalAny(Unit unit, Cell targetCell)
    {
        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;
        float dx = to.x - from.x;
        float dz = to.z - from.z;

        bool isDiagonal = Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dz)) < 0.1f;
        bool notSameCell = Mathf.Abs(dx) > 0.1f;

        Debug.Log($"Diagonal check: x={dx}, z={dz}, isDiagonal={isDiagonal}, notSameCell={notSameCell}");

        return isDiagonal && notSameCell;
    }


    private void HighlightPossibleMoves(Unit unit)
    {
        if (!IsUnitValid(unit)) return;

        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        if (unit.IsKing)
        {

            var kingMoves = GetKingMoves(unit);
            var jumpMoves = GetKingJumpMoves(unit);

            foreach (var cell in jumpMoves)
            {
                cell.SetSelect(_palette.AttackCell);
            }

            if (jumpMoves.Count == 0)
            {
                foreach (var cell in kingMoves)
                {
                    cell.SetSelect(_palette.MoveCell);
                }
            }

            return;
        }


        List<Cell> jumpMovesNormal = new List<Cell>();
        foreach (var cell in allCells)
        {
            if (CanJumpOver(unit, cell, out _))
            {
                jumpMovesNormal.Add(cell);
            }
        }


        if (jumpMovesNormal.Count > 0)
        {
            foreach (var cell in jumpMovesNormal)
            {
                cell.SetSelect(_palette.AttackCell);
                Debug.Log($"Regular piece JUMP to: {cell.transform.position}");
            }
            return;
        }


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


    private void MoveUnit(Unit unit, Cell targetCell)
    {
        if (!IsUnitValid(unit)) return;

        bool wasJump = false;
        Cell enemyCell = null;

        if (unit.IsKing)
            wasJump = CanKingJumpOver(unit, targetCell, out enemyCell);
        else
            wasJump = CanJumpOver(unit, targetCell, out enemyCell);

        
        if (wasJump && enemyCell != null)
        {
            Debug.Log($"Уничтожена шашка противника на {enemyCell.transform.position}");
            if (_unitsWithJumps.Contains(enemyCell.Unit))
            {
                _unitsWithJumps.Remove(enemyCell.Unit);
            }
            DestroyUnit(enemyCell.Unit);
            enemyCell.Unit = null;
        }

      
        unit.Cell.Unit = null;
        targetCell.Unit = unit;
        unit.Cell = targetCell;
        unit.transform.position = targetCell.transform.position + Vector3.up * 1f;

      
        if (!unit.IsKing && IsOnOppositeEdge(unit))
        {
            unit.IsKing = true;
            HighlightAsKing(unit);
            Debug.Log($"Шашка превратилась в дамку! {unit.Team} at {targetCell.transform.position}");
        }

      
        if (wasJump)
        {
            List<Cell> nextJumps = unit.IsKing ? GetKingJumpMoves(unit) : GetJumpMoves(unit);

            Debug.Log($"После прыжка: следующих прыжков доступно: {nextJumps.Count}");

            if (nextJumps.Count > 0)
            {
               
                _selectedUnit = unit;
                HighlightSelectedUnit(unit);
                ClearHighlights();

                
                foreach (var cell in nextJumps)
                    cell.SetSelect(_palette.AttackCell);

                Debug.Log($"Продолжение серии прыжков: {nextJumps.Count} возможных ходов");

                
                _unitsWithJumps.Clear();
                _unitsWithJumps.Add(unit);
                _mustAttack = true;

                Debug.Log($"Обновили _unitsWithJumps: теперь только 1 шашка может ходить");

                return; // Не переключаем ход
            }
        }

        // Завершаем ход (если не было продолжения прыжков)
        Debug.Log("Завершение хода");
        ResetAllSelection();
        SwitchTurn();
    }

    private void SwitchTurn()
    {
        Debug.Log($"=== SwitchTurn: {_currentTeam} -> {(_currentTeam == Team.Player1 ? Team.Player2 : Team.Player1)} ===");
        Team oldTeam = _currentTeam;
        ResetAllSelection();
        var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        foreach (var unit in allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy || unit.Cell == null)
            {
                // Нашли "мертвую" шашку - уничтожаем, долой зомби
                if (unit != null && unit.gameObject != null)
                    Destroy(unit.gameObject);
            }
        }

        _currentTeam = _currentTeam == Team.Player1 ? Team.Player2 : Team.Player1;
        OnTurnChanged?.Invoke(_currentTeam);

        Debug.Log($"=== СМЕНА ХОДА: теперь ходит {_currentTeam} ===");

       
        Debug.Log("Сбрасываем состояние: _selectedUnit, _mustAttack, _unitsWithJumps");
        _selectedUnit = null;
        _mustAttack = false;
        _unitsWithJumps.Clear();

        PrepareTurn();
        CleanupDestroyedUnits();
    }

    private void ResetAllSelection()
    {
        
        if (_selectedUnit != null)
        {
            UnhighlightSelectedUnit(_selectedUnit);
            _selectedUnit = null;
        }


        _unitsWithJumps.RemoveAll(unit => unit == null);

        foreach (var unit in _unitsWithJumps)
        {
            UnhighlightAttackUnit(unit);
        }

        ClearHighlights();
        _unitsWithJumps.Clear();
        _mustAttack = false;
    }


    private bool CanJumpOver(Unit unit, Cell targetCell, out Cell enemyCell)
    {

        enemyCell = null;

        if (targetCell.Unit != null)
        {
            Debug.Log($"Целевая клетка занята - прыжок невозможен");
            return false;
        }

        Vector3 from = unit.Cell.transform.position;
        Vector3 to = targetCell.transform.position;

        Vector2Int fromCoords = _cellManager.WorldToBoardCoords(from);
        Vector2Int toCoords = _cellManager.WorldToBoardCoords(to);

        Vector3 jumpDir = to - from;

        if (Mathf.Abs(jumpDir.magnitude - Mathf.Sqrt(32)) > 0.1f)
        {
            Debug.Log($"Неправильное расстояние для прыжка: {jumpDir.magnitude}");
            return false;
        }

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
                    Debug.Log($"Найден враг! Прыжок возможен с ({fromCoords.x},{fromCoords.y}) на ({toCoords.x},{toCoords.y})");
                    return true;
                }
                else if (cell.Unit != null)
                {
                    Debug.Log("На пути своя шашка - прыжок невозможен");
                    return false;
                }
                else
                {
                    Debug.Log("На пути пустая клетка - прыжок невозможен");
                    return false;
                }
            }
        }

        Debug.Log("Не найдена промежуточная клетка для прыжка");
        return false;
    }


    private List<Cell> GetJumpMoves(Unit unit)
    {
        if (!IsUnitValid(unit)) return new List<Cell>();

        List<Cell> jumps = new();
        var allCells = FindObjectsByType<Cell>(FindObjectsSortMode.None);

        Debug.Log($"=== GetJumpMoves для {unit.Team} at {unit.Cell.transform.position} ===");

        Vector2Int unitCoords = _cellManager.WorldToBoardCoords(unit.Cell.transform.position);
        Debug.Log($"Позиция шашки на доске: ({unitCoords.x},{unitCoords.y})");

        foreach (var cell in allCells)
        {
            Vector2Int cellCoords = _cellManager.WorldToBoardCoords(cell.transform.position);

            if (CanJumpOver(unit, cell, out Cell enemyCell))
            {
                jumps.Add(cell);
                Vector2Int enemyCoords = _cellManager.WorldToBoardCoords(enemyCell.transform.position);
                Debug.Log($"  Найден прыжок: ({unitCoords.x},{unitCoords.y}) -> ({cellCoords.x},{cellCoords.y}) через врага на ({enemyCoords.x},{enemyCoords.y})");
            }
        }

        Debug.Log($"GetJumpMoves для {unit.Team} at {unit.Cell.transform.position}: найдено {jumps.Count} прыжков");
        return jumps;
    }

    private bool IsOnOppositeEdge(Unit unit)
    {
        if (!IsUnitValid(unit)) return false;

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
        if (!IsUnitValid(unit)) return new List<Cell>();

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
        if (!IsUnitValid(unit)) return new List<Cell>();

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

    private void HighlightAttackUnit(Unit unit)
    {
        Vector3 currentPos = unit.transform.position;
        unit.transform.position = new Vector3(currentPos.x, currentPos.y + 0.3f, currentPos.z); 
    }

    private void UnhighlightAttackUnit(Unit unit)
    {
        Vector3 currentPos = unit.transform.position;
        float baseHeight = unit.Cell.transform.position.y + 1f;
        unit.transform.position = new Vector3(currentPos.x, baseHeight, currentPos.z);
    }

    private void PrepareTurn()
    {
        _unitsWithJumps.Clear();
        _mustAttack = false;

        System.GC.Collect();

        var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None)
        .Where(u => u != null && u.gameObject != null && u.gameObject.activeInHierarchy && u.Cell != null)
        .ToArray();

        Debug.Log($"=== PrepareTurn для {_currentTeam} ===");
        Debug.Log($"Всего шашек на поле: {allUnits.Length}");

        ClearHighlights();
     
        foreach (var unit in allUnits)
        {
       
            if (unit == null || !unit.gameObject.activeInHierarchy || unit.Cell == null)
                continue;

            if (unit.Team == _currentTeam)
            {
                UnhighlightAttackUnit(unit);
            }
        }
        ClearHighlights(); 

        int currentTeamUnits = 0;
        foreach (var unit in allUnits)
        {
            if (unit == null || !unit.gameObject.activeInHierarchy || unit.Cell == null || unit.Team != _currentTeam || unit.gameObject == null)
                continue;

            currentTeamUnits++;
            Vector2Int coords = _cellManager.WorldToBoardCoords(unit.Cell.transform.position);
            Debug.Log($"Проверяем шашку: {unit.Team} at ({coords.x},{coords.y}) - King: {unit.IsKing}");

            List<Cell> jumps;
            if (unit.IsKing)
                jumps = GetKingJumpMoves(unit);
            else
                jumps = GetJumpMoves(unit);

            Debug.Log($"Шашка {unit.Team} at {unit.Cell.transform.position} - прыжков: {jumps.Count}");

            if (jumps.Count > 0)
            {
                _unitsWithJumps.Add(unit);
                HighlightAttackUnit(unit); // Приподнимаем шашку
                _mustAttack = true;
                Debug.Log($"ДОБАВЛЕНА в _unitsWithJumps: {unit.Team} at {unit.Cell.transform.position}");

                // СРАЗУ ПОДСВЕЧИВАЕМ КЛЕТКИ ДЛЯ ПРЫЖКА КРАСНЫМ ЦВЕТОМ
                foreach (var jumpCell in jumps)
                {
                    jumpCell.SetSelect(_palette.AttackCell);
                    Vector2Int jumpCoords = _cellManager.WorldToBoardCoords(jumpCell.transform.position);
                    Debug.Log($"  -> Подсвечена клетка для прыжка: ({jumpCoords.x},{jumpCoords.y})");
                }
            }
        }

        Debug.Log($"Всего шашек {_currentTeam}: {currentTeamUnits}");
        Debug.Log($"Итог: {_unitsWithJumps.Count} шашек могут атаковать, обязательная атака: {_mustAttack}");

        foreach (var unit in _unitsWithJumps)
        {
            if (unit != null && unit.Cell != null)
            {
                Vector2Int coords = _cellManager.WorldToBoardCoords(unit.Cell.transform.position);
                Debug.Log($"Может атаковать: {unit.Team} at ({coords.x},{coords.y})");
            }
        }
    }

    private void DebugUnitPosition(Unit unit, string action)
    {

        if (!IsUnitValid(unit))
        {
            Debug.Log($"{action}: ЮНИТ НЕВАЛИДЕН!");
            return;
        }

        Vector3 pos = unit.Cell.transform.position;
        Vector2Int coords = _cellManager.WorldToBoardCoords(pos);
        Debug.Log($"{action}: {unit.Team} at WORLD({pos.x:F1}, {pos.z:F1}) -> BOARD({coords.x},{coords.y}) - King: {unit.IsKing}");
    }

    private bool IsUnitValid(Unit unit)
    {
        return unit != null && unit.gameObject != null && unit.Cell != null;
    }

    private void DestroyUnit(Unit unit)
    {
        if (unit == null) return;

        Debug.Log($" УНИЧТОЖАЕМ ШАШКУ: {unit.Team} at {unit.Cell?.transform.position}");

        if (_unitsWithJumps.Contains(unit))
        {
            _unitsWithJumps.Remove(unit);
            Debug.Log($"   Удалена из _unitsWithJumps");
        }

        if (_selectedUnit == unit)
        {
            _selectedUnit = null;
            Debug.Log($"   Сброшен _selectedUnit");
        }

        if (unit.Cell != null && unit.Cell.Unit == unit)
        {
            unit.Cell.Unit = null;
            Debug.Log($"   Очищена ссылка на клетке {unit.Cell.transform.position}");
        }

        if (unit.gameObject != null)
        {
            unit.gameObject.SetActive(false);
            Debug.Log($"   Деактивирован gameObject");

            Destroy(unit.gameObject);
            Debug.Log($"   Уничтожен gameObject");
        }

        Debug.Log($"ШАШКА ПОЛНОСТЬЮ УНИЧТОЖЕНА");
        CleanupDestroyedUnits();
    }


    private void CleanupDestroyedUnits()
    {
        var allUnits = FindObjectsByType<Unit>(FindObjectsSortMode.None);
        int destroyedCount = 0;

        foreach (var unit in allUnits)
        {
            if (unit == null) continue;

            if (unit.gameObject == null || !unit.gameObject.activeInHierarchy)
            {
                Destroy(unit.gameObject);
                destroyedCount++;
            }
            else if (unit.Cell == null)
            {
                // Шашка без клетки - явно мертвая
                Destroy(unit.gameObject);
                destroyedCount++;
            }
        }

        if (destroyedCount > 0)
            Debug.Log($"Очищено {destroyedCount} мертвых юнитов");
    }


}