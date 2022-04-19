using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu
{
    public class PauseMenu : MonoBehaviour
    {
        private static bool _gameIsPaused;
        
        public GameObject pauseMenu;

        [SerializeField] private Slider volumeSlider;


        private void Start()
        {
            SetVolume(GameManager.Instance.globalAudioVolume);
            switch (GameManager.Instance.gameMode)
            {
                case GameManager.GameMode.Menu:
                    volumeSlider.value = GameManager.Instance.globalAudioVolume;
                    break;
                case GameManager.GameMode.Editing:
                    volumeSlider.value = GameManager.Instance.globalAudioVolume;
                    break;
                case GameManager.GameMode.Explore:
                    volumeSlider.value = GameManager.Instance.globalAudioVolume;
                    break;
                case GameManager.GameMode.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

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

        public void SetVolume(float volume)
        {
            GameManager.Instance.globalAudioVolume = volume;
            switch (GameManager.Instance.gameMode)
            {
                case GameManager.GameMode.Menu:
                    GameManager.Instance.MainMenuMusicTriggerer.SetVolume(volume);
                    break;
                case GameManager.GameMode.Editing:
                    GameManager.Instance.EditingMusicTriggerer.SetVolume(volume);
                    break;
                case GameManager.GameMode.Explore:
                    GameManager.Instance.ExplorationMusicTriggerer.SetVolume(volume);
                    break;
                case GameManager.GameMode.Unknown:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}