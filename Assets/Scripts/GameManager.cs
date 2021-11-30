using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private string _systemToLoad;
    private bool _isNew;

    public enum GameMode {Menu, Editing, Explore, Unknown}

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            
            // apply some quality settings 
            QualitySettings.vSyncCount = 1;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Camera GetMainCamera()
    {
        return Camera.main;
    }

    public GameMode GetGameMode()
    {
        if (SceneManager.GetActiveScene().name == SceneManager.GetSceneAt(0).name)
            return GameMode.Menu;
        if (SceneManager.GetActiveScene().name == SceneManager.GetSceneAt(1).name)
            return GameMode.Editing;
        if (SceneManager.GetActiveScene().name == SceneManager.GetSceneAt(2).name)
            return GameMode.Explore;
        return GameMode.Unknown;
    }

    public void SetSystemToLoad(string systemName, bool isNew)
    {
        _isNew = isNew;
        _systemToLoad = systemName;
    }

    public (string name, bool isNew) GetSystemToLoad()
    {
        var temp = _systemToLoad;
        var temp2 = _isNew;
        _systemToLoad = null;
        _isNew = false;
        return (temp, temp2);
    }
}
