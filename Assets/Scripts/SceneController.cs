using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
    public void OpenGameScene()
    {
        SceneManager.LoadScene("GameScene", LoadSceneMode.Additive);
    }
}