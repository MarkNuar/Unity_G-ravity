using System;
using System.Collections.Generic;
using System.IO;
using CBodies.Settings;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

// SINGLETON
public class SystemUtils : MonoBehaviour
{ 
    public static SystemUtils Instance; 
    
    private string _storePath = null;
    
    // SHAPES 
    [Header("Shapes")]
    public Shape baseShape;
    public RockShape rockShape;
    public GaseousShape gaseousShape;
    public StarShape starShape;
    
    // SHADING 
    [Header("Shading")]
    public Shading baseShading;
    public RockShading rockShading;
    public GaseousShading gaseousShading;
    public StarShading starShading;
    
    // PHYSICS
    [Header("Physics")]
    public Physics basePhysics;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);
            
            // Save the store path
            _storePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SaveSystem(SystemSettings systemSettings)
    {
        Debug.Log("Saving system: " + systemSettings.systemName);
        
        if (_storePath == null) return;

        // Add the newly created system name to the system names list
        var systemNamesPath = _storePath + "names_of_systems";
        SavedSysNames savedNames = new SavedSysNames();
        if (File.Exists(systemNamesPath))
        {
            StreamReader namesReader = new StreamReader(systemNamesPath);
            savedNames = JsonUtility.FromJson<SavedSysNames>(namesReader.ReadToEnd());
            namesReader.Close();
        }
        savedNames.savedSysNames.Add(systemSettings.systemName);
        Debug.Log(savedNames.savedSysNames[0]);
        StreamWriter namesWriter = new StreamWriter(systemNamesPath, false);
        var namesJson = JsonUtility.ToJson(savedNames);
        namesWriter.WriteLine(namesJson);
        namesWriter.Close();
        
        
        // Stores the types of the newly created system cbodies
        SystemTypes systemTypes = new SystemTypes
        {
            types = new CBodySettings.CBodyType[systemSettings.cBodiesSettings.Count]
        };
        for (var i = 0; i < systemSettings.cBodiesSettings.Count; i++)
        {
            systemTypes.types[i] = systemSettings.cBodiesSettings[i].cBodyType;
        }
        var systemTypesPath = _storePath + systemSettings.systemName + "_types";
        StreamWriter typesWriter = new StreamWriter(systemTypesPath, false);
        var typesJson = JsonUtility.ToJson(systemTypes);
        typesWriter.WriteLine(typesJson);
        typesWriter.Close();
        
        
        // Stores the system settings
        var systemSettingsPath = _storePath + systemSettings.systemName + "_settings";
        StreamWriter settingsWriter = new StreamWriter(systemSettingsPath, false);
        var settingsJson = JsonUtility.ToJson(systemSettings);
        settingsWriter.WriteLine(settingsJson);
        settingsWriter.Close();
    }

    public SystemSettings LoadSystem(string systemName)
    {
        var systemTypesPath = _storePath + systemName + "_types";
        var systemSettingsPath = _storePath + systemName + "_settings";
        
        if (File.Exists(systemSettingsPath) && File.Exists(systemTypesPath))
        {
            // There exists already a previous saved state
            StreamReader typesReader = new StreamReader(systemTypesPath);
            SystemTypes loadedTypes = JsonUtility.FromJson<SystemTypes>(typesReader.ReadToEnd());

            if (loadedTypes != null)
            {
                Debug.Log(loadedTypes.types);
                
                SystemSettings systemSettings = new SystemSettings
                {
                    cBodiesSettings = new List<CBodySettings>()
                };
                foreach (CBodySettings.CBodyType type in loadedTypes.types)
                {
                    CBodySettings cBodySettings = new CBodySettings();

                    (Shape shape, Shading shading, Physics physics) = GetShapeShadingPhysics(type);
                    cBodySettings.shape = shape;
                    cBodySettings.shading = shading;
                    cBodySettings.physics = physics;
                    
                    systemSettings.cBodiesSettings.Add(cBodySettings);
                }
                
                StreamReader settingsReader = new StreamReader(systemSettingsPath);
                JsonUtility.FromJsonOverwrite(settingsReader.ReadToEnd(), systemSettings);

                return systemSettings;
            }
            else
            {
                Debug.LogError("System types not correctly loaded");
                return null;
            }
        }
        Debug.LogError("No saved system data found");
        return null;
    }

    public void DeleteSystem(string systemName)
    {
        // todo 
        // remove the system name from the names list 
        // delete the files containing system types and system settings 
    }

    public (Shape shape, Shading shading, Physics physics) GetShapeShadingPhysics(CBodySettings.CBodyType type)
    {
        Shape shape = null;
        Shading shading = null;
        Physics physics = Instantiate(basePhysics);
        switch (type)
        {
            case CBodySettings.CBodyType.Base:
                shape = Instantiate(baseShape);
                shading = Instantiate(baseShading);
                break;
            case CBodySettings.CBodyType.Rocky:
                shape = Instantiate(rockShape);
                shading = Instantiate(rockShading);
                break;
            case CBodySettings.CBodyType.Gaseous:
                shape = Instantiate(gaseousShape);
                shading = Instantiate(gaseousShading);
                break;
            case CBodySettings.CBodyType.Star:
                shape = Instantiate(starShape);
                shading = Instantiate(starShading);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        return (shape, shading, physics);
    }

    public List<string> GetSystemNames()
    {
        var systemNamesPath = _storePath + "names_of_systems";
        SavedSysNames savedNames = new SavedSysNames();
        if (!File.Exists(systemNamesPath)) return savedNames.savedSysNames;
        StreamReader reader = new StreamReader(systemNamesPath);
        savedNames = JsonUtility.FromJson<SavedSysNames>(reader.ReadToEnd());
        return savedNames.savedSysNames;
    }
    
    // Utility class for serializing system cBodies types
    [Serializable]
    private class SystemTypes
    {
        public CBodySettings.CBodyType[] types; 
    }

    // Utility class for serializing systems names
    [Serializable]
    private class SavedSysNames
    {
        public List<string> savedSysNames = new List<string>();
    }
}