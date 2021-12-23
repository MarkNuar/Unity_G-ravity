using System.Collections.Generic;
using TMPro;
using UnityEngine;

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
        private readonly List<string> _editLoadedSystems = new List<string>();
        private readonly List<string> _exploreLoadedSystems = new List<string>();


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
                if (!_editLoadedSystems.Contains(systemName))
                {
                    GameObject el = Instantiate(scrollViewElementPrefab, editScrollView, true);
                    el.transform.localScale = Vector3.one;
                    ListElementHelper helper = el.GetComponent<ListElementHelper>();
                    helper.text.text = systemName;
                    helper.button.onClick.AddListener(() => LoadSystemEditingScene(systemName, false));
                    _editLoadedSystems.Add(systemName);
                }
            }
        }

        public void OpenExploreSelection()
        {
            ShowPanel(2);
            
            List<string> savedSystems = SystemUtils.Instance.GetSystemNames();
            
            foreach (var systemName in savedSystems)
            {
                if (!_exploreLoadedSystems.Contains(systemName))
                {
                    GameObject el = Instantiate(scrollViewElementPrefab, exploreScrollView, true);
                    el.transform.localScale = Vector3.one;
                    ListElementHelper helper = el.GetComponent<ListElementHelper>();
                    helper.text.text = systemName;
                    helper.button.onClick.AddListener(() => LoadSystemExplorationScene(systemName));
                    _exploreLoadedSystems.Add(systemName);
                }
                
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
            _editLoadedSystems.Clear();
            _exploreLoadedSystems.Clear();
            GameManager.Instance.SetSystemToLoad(systemName, isNew);
            //SceneManager.LoadScene("Scenes/SystemEditing", LoadSceneMode.Single);
            GameManager.Instance.LoadScene(1);
        }
        
        private void LoadSystemExplorationScene(string systemName)
        {
            _cachedNames.Clear();
            _editLoadedSystems.Clear();
            _exploreLoadedSystems.Clear();
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
