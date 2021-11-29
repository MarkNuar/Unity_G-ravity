using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private string _systemToLoad;

    public enum GameMode {Menu, Editing, Explore, Unknown}

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(Instance);

        
        // apply some quality settings 
        QualitySettings.vSyncCount = 1;
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

    public void SetSystemToLoad(string systemName)
    {
        _systemToLoad = systemName;
    }

    public string GetSystemToLoad()
    {
        var temp = _systemToLoad;
        _systemToLoad = null;
        return temp;
    }
}
