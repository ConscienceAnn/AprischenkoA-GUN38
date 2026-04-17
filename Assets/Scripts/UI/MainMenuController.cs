using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI
{
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _level1Button;
        [SerializeField] private Button _level2Button;
        [SerializeField] private Button _level3Button;
        [SerializeField] private Button _quitButton;

        private void Awake()
        {
            _level1Button?.onClick.AddListener(() => LoadLevel("Level1"));
            _level2Button?.onClick.AddListener(() => LoadLevel("Level2"));
            _level3Button?.onClick.AddListener(() => LoadLevel("Level3"));
            _quitButton?.onClick.AddListener(Quit);
        }

        private void LoadLevel(string levelName)
        {
            SceneManager.LoadScene(levelName);
        }

        private void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}