
using UnityEngine;
using Zenject;

public class SceneInstaller : MonoInstaller
{
    [SerializeField]
    private CellManager _cellManager;

    [SerializeField]
    private CellPaletteSettings _cellPaletteSettings;
    public override void InstallBindings()
    {
        GameInput gameInput = new GameInput();
        gameInput.Game.Enable();

        Container.BindInstance(_cellManager).AsSingle();
        Container.BindInstance(_cellPaletteSettings).AsSingle();
        Container.BindInstance(gameInput).AsSingle();
        Container.BindInstance(gameInput.Game).AsSingle(); //вопросики
        Container.Bind<BattleController>().FromComponentInHierarchy().AsSingle();
        Container.Bind<InputManager>().FromComponentInHierarchy().AsSingle();
    }
}
 
