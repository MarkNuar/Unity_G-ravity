using UnityEngine;

namespace Game.UI.Menu
{
    public class PauseMenu : MonoBehaviour
    {
        private static bool _gameIsPaused;
        
        public GameObject pauseMenu;
        
        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.Escape)) return;
            if (_gameIsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
        
        private void Pause()
        {
            pauseMenu.SetActive(true);
            Time.timeScale = 0f;
            _gameIsPaused = true;
            GameManager.Instance.gamePaused = true;
        }
        
        public void Resume()
        {
            pauseMenu.SetActive(false);
            Time.timeScale = 1f;
            _gameIsPaused = false;
            GameManager.Instance.gamePaused = false;
        }

        public void LoadMainMenu()
        {
            Resume();
            //SceneManager.LoadScene("Scenes/MainMenu", LoadSceneMode.Single);
            GameManager.Instance.LoadScene(0);
        }
    }
}