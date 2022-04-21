using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public List<GameObject> panels;

        public RectTransform editScrollView;
        public RectTransform exploreScrollView;

        public GameObject scrollViewElementPrefab;

        public TMP_InputField systemNameInput;
        public TMP_Text systemNameError;

        private readonly List<string> _cachedNames = new List<string>();
        private readonly List<string> _loadedEditingSystems = new List<string>();
        private readonly List<string> _loadedExplorationSystems = new List<string>();
        
        
        [SerializeField] private Slider volumeSlider;
        
        private void Start()
        {
            _cachedNames.Clear();
            _loadedEditingSystems.Clear();
            _loadedExplorationSystems.Clear();

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

        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenEditSelection()
        {
            _cachedNames.Clear();

            ShowPanel(1);

            List<string> savedSystems = SystemUtils.Instance.GetSystemNames();

            _cachedNames.AddRange(savedSystems);
            
            foreach (var systemName in savedSystems)
            {
                if(_loadedEditingSystems.Contains(systemName)) continue;

                GameObject el = Instantiate(scrollViewElementPrefab, editScrollView, true);
                el.transform.localScale = Vector3.one;
                ListElementHelper helper = el.GetComponent<ListElementHelper>();
                helper.text.text = systemName;
                helper.button.onClick.AddListener(() => LoadSystemEditingScene(systemName, false));
                helper.deleteButton.onClick.AddListener(() => DeleteSystem(systemName, el));
                _loadedEditingSystems.Add(systemName);
            }
        }

        public void DeleteSystem(string systemName, GameObject listElement)
        {
            Destroy(listElement);
            SystemUtils.Instance.DeleteSystem(systemName);
            OpenEditSelection();
        }

        public void OpenExploreSelection()
        {
            _cachedNames.Clear();

            ShowPanel(2);
            
            List<string> savedSystems = SystemUtils.Instance.GetSystemNames();
            
            _cachedNames.AddRange(savedSystems);
            
            foreach (var systemName in savedSystems)
            {
                if(_loadedExplorationSystems.Contains(systemName)) continue;
                
                GameObject el = Instantiate(scrollViewElementPrefab, exploreScrollView, true);
                el.transform.localScale = Vector3.one;
                ListElementHelper helper = el.GetComponent<ListElementHelper>();
                helper.text.text = systemName;
                helper.button.onClick.AddListener(() => LoadSystemExplorationScene(systemName));
                _loadedExplorationSystems.Add(systemName);
            }
        }
        
        public void OpenMainMenu()
        {
            ShowPanel(0);
        }

        public void OpenNewSystemNameSelection()
        {
            ShowPanel(3);
            
            _cachedNames.AddRange(SystemUtils.Instance.GetSystemNames());
        }

        public void ValidateName()
        {
            if (systemNameInput.text == "")
            {
                systemNameError.text = "Name must be not empty";
            }
            else if (_cachedNames.Contains(systemNameInput.text))
            {
                systemNameError.text = "Name already used";
            }
            else
            {
                LoadSystemEditingScene(systemNameInput.text, true);
            }
        }
        
        private void LoadSystemEditingScene(string systemName, bool isNew)
        {
            _cachedNames.Clear();
            _loadedEditingSystems.Clear();
            _loadedExplorationSystems.Clear();
            GameManager.Instance.SetSystemToLoad(systemName, isNew);
            //SceneManager.LoadScene("Scenes/SystemEditing", LoadSceneMode.Single);
            GameManager.Instance.LoadScene(1);
        }
        
        private void LoadSystemExplorationScene(string systemName)
        {
            _cachedNames.Clear();
            _loadedEditingSystems.Clear();
            _loadedExplorationSystems.Clear();
            GameManager.Instance.SetSystemToLoad(systemName, false);
            //SceneManager.LoadScene("Scenes/SystemExploration", LoadSceneMode.Single);
            GameManager.Instance.LoadScene(2);
        }
        
        private void ShowPanel(int position)
        {
            for (var i = 0; i < panels.Count; i ++)
            {
                panels[i].SetActive(i == position);
            }
        }

        private void OverlayPanel(int position, bool overlay)
        {
            panels[position].SetActive(overlay);
        }
    }
}
