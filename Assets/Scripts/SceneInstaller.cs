
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField]
    private CellManager _cellManager;

    [SerializeField]
    private CellPaletteSettings _cellPaletteSettings;

    //[SerializeField] private GameInput _gameInput;


    public override void InstallBindings()
    {
        if (_cellManager == null)
        {
            Debug.LogError("CellManager is NOT assigned in SceneInstaller!");
            return;
        }

        if (_cellPaletteSettings == null)
        {
            Debug.LogError("CellPaletteSettings is NOT assigned in SceneInstaller!");
            return;
        }

        // СОЗДАЕМ GameInput ПРЯМО В КОДЕ
        GameInput gameInput = new GameInput();
        gameInput.Game.Enable();

        // Привязываем зависимости
        Container.BindInstance(_cellManager).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();
        Container.BindInstance(gameInput).AsSingle(); // Привязываем созданный экземпляр

        Container.Bind<BattleController>().FromComponentInHierarchy().AsSingle();
        Debug.Log("SceneInstaller: All dependencies bound successfully with GameInput");
    }
}
 
