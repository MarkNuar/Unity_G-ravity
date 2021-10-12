using System.IO;
using CBodies.Data;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string storePath = null;

    private void Awake()
    {
        if (Instance != null) return;
        Instance = this;
        DontDestroyOnLoad(Instance);
        storePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
        
        // apply some quality settings 
        QualitySettings.vSyncCount = 1;
    }

    public void SaveSystem(SystemData systemData)
    {
        if (storePath == null) return;
        var path = storePath + systemData.systemName;
        var writer = new StreamWriter(path, false);
        var res = JsonUtility.ToJson(systemData);
        writer.WriteLine(res);
        writer.Close();
    }

    public SystemData LoadSystem(string systemName)
    {
        var path = storePath + systemName;
        Debug.Log(path);
        if (File.Exists(path))
        {
            // There exists already a previous saved state
            var reader = new StreamReader(path);
            var loadedSettings = JsonUtility.FromJson<SystemData>(reader.ReadToEnd());
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
}
