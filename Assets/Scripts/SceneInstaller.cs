
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
        // ПРОВЕРЬ и заполни эти поля в инспекторе!
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

        //if (_gameInput == null)
        //{
        //    Debug.LogError("GameInput is NULL! Create InputActions asset and assign it here.");
        //    return;
        //}

        // Привязываем зависимости
        Container.BindInstance(_cellManager).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();
        //Container.BindInstance(_gameInput).AsSingle();

        // BattleController будет найден в иерархии
        Container.Bind<BattleController>().FromComponentInHierarchy().AsSingle();

        Debug.Log("SceneInstaller: All dependencies bound successfully");
    }
}
 
