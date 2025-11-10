public class SelectCellCommand : IGameplayCommand
{
    private BattleController _battleController;

    public SelectCellCommand(BattleController controller)
    {
        _battleController = controller;
    }

    public void Interact(Cell cell)
    {
        _battleController.HandleCellClick(cell);
    }
}