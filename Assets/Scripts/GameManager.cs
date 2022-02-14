using System.IO;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    private string _systemToLoad;
    private bool _isNew;

    public bool gamePaused = false;

    public enum GameMode {Menu, Editing, Explore, Unknown}
    public GameMode gameMode; 
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);

            // apply some quality settings 
            QualitySettings.vSyncCount = 1;
            Application.targetFrameRate = 30;
            
            JsonConvert.DefaultSettings().Converters.Add(new ColorConverter());
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

    public void LoadScene(int index)
    {
        gameMode = index switch
        {
            0 => GameMode.Menu,
            1 => GameMode.Editing,
            2 => GameMode.Explore,
            _ => GameMode.Unknown
        };
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }
}
