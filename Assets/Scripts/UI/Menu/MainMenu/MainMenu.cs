using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public List<GameObject> panels;

        public RectTransform editScrollView;
        public RectTransform exploreScrollView;

        public GameObject scrollViewElementPrefab;

        public TMP_InputField systemNameInput;
        public TMP_Text systemNameError;

        private List<string> _cachedNames;
        
        
        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenEditSelection()
        {
            ShowPanel(1);

            List<string> savedSystems = SystemUtils.Instance.GetSystemNames();

            foreach (var systemName in savedSystems)
            {
                GameObject el = Instantiate(scrollViewElementPrefab, editScrollView, true);
                ListElementHelper helper = el.GetComponent<ListElementHelper>();
                helper.text.text = systemName;
                helper.button.onClick.AddListener(() => LoadSystemEditingScene(systemName));
            }
        }

        public void OpenExploreSelection()
        {
            ShowPanel(2);
            
            List<string> savedSystems = SystemUtils.Instance.GetSystemNames();

            foreach (var systemName in savedSystems)
            {
                GameObject el = Instantiate(scrollViewElementPrefab, exploreScrollView, true);
                ListElementHelper helper = el.GetComponent<ListElementHelper>();
                helper.text.text = systemName;
                helper.button.onClick.AddListener(() => LoadSystemExplorationScene(systemName));
            }
        }
        
        public void OpenMainMenu()
        {
            ShowPanel(0);
        }

        public void OpenNewSystemNameSelection()
        {
            ShowPanel(3);
            
            _cachedNames = SystemUtils.Instance.GetSystemNames();
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
                LoadSystemEditingScene(systemNameInput.text);
            }
        }
        
        private void LoadSystemEditingScene(string systemName)
        {
            GameManager.Instance.SetSystemToLoad(systemName);
            SceneManager.LoadScene("Scenes/SystemEditing", LoadSceneMode.Single);
        }
        
        private void LoadSystemExplorationScene(string systemName)
        {
            GameManager.Instance.SetSystemToLoad(systemName);
            SceneManager.LoadScene("Scenes/SystemExploration", LoadSceneMode.Single);
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
