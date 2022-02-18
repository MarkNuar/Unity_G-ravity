// precise frame rate from:
// https://blog.unity.com/technology/precise-framerates-in-unity

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections;
using System.Threading;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Tooltip("Target frame rate of the game")]
    public float targetRate = 30.0f;
    private float _currentFrameTime;
    
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

            // // apply some quality settings 
            // QualitySettings.vSyncCount = 0;
            // Application.targetFrameRate = 300;
            
            JsonConvert.DefaultSettings().Converters.Add(new ColorConverter());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 30;
        // _currentFrameTime = Time.realtimeSinceStartup;
        // StartCoroutine("WaitForNextFrame");
    }

    private IEnumerator WaitForNextFrame()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            _currentFrameTime += 1.0f / targetRate;
            var t = Time.realtimeSinceStartup;
            var sleepTime = _currentFrameTime - t - 0.01f;
            if (sleepTime > 0)
                Thread.Sleep((int)(sleepTime * 1000));
            while (t < _currentFrameTime)
                t = Time.realtimeSinceStartup;
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
