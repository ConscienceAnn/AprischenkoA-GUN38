
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
    [SerializeField]
    private CellManager _cellManager;

    [SerializeField]
    private CellPaletteSettings _cellPaletteSettings;
    public override void InstallBindings()
    {
        // 1. Создаём Controls
        var controls = new GameControls();

        // 2. Прокидываем ВЕСЬ объект Controls в контейнер (на случай, если понадобится)
        Container.BindInstance(controls).AsSingle();

        // 3. Прокидываем КАРТУ ЭКШЕНОВ (Game) — именно то, что нужно для InputManager
        Container.BindInstance(controls.Game).AsSingle();

        // 4. Другие биндинги
        Container.BindInstance(_cellPaletteSettings).AsSingle();
        Container.BindInstance(_cellManager).AsSingle();

        // 5. Подписка на клик по клетке (как у тебя было)
        _cellManager.OnCellClicked.AddListener(cell =>
        {
            cell.SetSelect(_cellPaletteSettings.SelectCell);
        });
    }

    private void CellManagerOnOnCellClicked(Cell obj)
    {
        obj.SetSelect(_cellPaletteSettings.SelectCell);
    }

}
