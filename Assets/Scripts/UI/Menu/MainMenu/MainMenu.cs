using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UI.Menu.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        public List<GameObject> panels;
            
        public void QuitGame()
        {
            Application.Quit();
        }

        public void OpenSystemSelectionMenu()
        {
            panels[0].SetActive(false);
            panels[1].SetActive(true);
        }

        public void OpenMainMenu()
        {
            panels[0].SetActive(true);
            panels[1].SetActive(false);
        }

        public void LoadSystemCreationScene()
        {
            SceneManager.LoadScene("Scenes/SystemEditing", LoadSceneMode.Single);
        }
    }
}
