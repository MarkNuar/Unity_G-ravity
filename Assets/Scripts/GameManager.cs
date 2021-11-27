using System.IO;
using CBodies.CBodySettings;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string storePath = null;
    
    public enum GameMode {Menu, Editing, Explore, Unknown}

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(Instance);
        storePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
        
        // apply some quality settings 
        QualitySettings.vSyncCount = 1;
    }

    public void SaveSystem(SystemSettings systemSettings)
    {
        if (storePath == null) return;
        var path = storePath + systemSettings.systemName;
        var writer = new StreamWriter(path, false);
        var res = JsonUtility.ToJson(systemSettings);
        writer.WriteLine(res);
        writer.Close();
    }

    public SystemSettings LoadSystem(string systemName)
    {
        var path = storePath + systemName;
        Debug.Log(path);
        if (File.Exists(path))
        {
            // There exists already a previous saved state
            var reader = new StreamReader(path);
            var loadedSettings = JsonUtility.FromJson<SystemSettings>(reader.ReadToEnd());
            if (loadedSettings != null)
            {
                return loadedSettings;
            }
            else
            {
                Debug.LogError("System not correctly loaded");
                return null;
            }
        }
        Debug.LogError("No saved system data found");
        return null;
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
}
