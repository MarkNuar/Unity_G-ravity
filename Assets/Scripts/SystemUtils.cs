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
        if (_storePath == null) return;

        // Add the newly created system name to the system names list
        var systemNamesPath = _storePath + "names_of_systems";
        SavedSystemsNames savedNames = new SavedSystemsNames();
        if (File.Exists(systemNamesPath))
        {
            StreamReader namesReader = new StreamReader(systemNamesPath);
            savedNames = JsonUtility.FromJson<SavedSystemsNames>(namesReader.ReadToEnd());
            namesReader.Close();
        }
        if (!savedNames.savedSysNames.Contains(systemSettings.systemName))
        {
            savedNames.savedSysNames.Add(systemSettings.systemName);
        }
        StreamWriter namesWriter = new StreamWriter(systemNamesPath, false);
        var namesJson = JsonUtility.ToJson(savedNames);
        namesWriter.WriteLine(namesJson);
        namesWriter.Close();
        
        
        // Stores the types of the newly created system cbodies
        CBodiesTypes cBodiesTypes = new CBodiesTypes
        {
            types = new CBodySettings.CBodyType[systemSettings.cBodiesSettings.Count]
        };
        for (var i = 0; i < systemSettings.cBodiesSettings.Count; i++)
        {
            cBodiesTypes.types[i] = systemSettings.cBodiesSettings[i].cBodyType;
        }
        var cBodiesTypesPath = _storePath + systemSettings.systemName + "_cBodies_types";
        StreamWriter typesWriter = new StreamWriter(cBodiesTypesPath, false);
        var typesJson = JsonUtility.ToJson(cBodiesTypes);
        typesWriter.WriteLine(typesJson);
        typesWriter.Close();


        // Stores cBody settings for each cBody
        CBodiesSettings cBodiesSettings = new CBodiesSettings();
        foreach (CBodySettings cBodySettings in systemSettings.cBodiesSettings)
        {
            cBodiesSettings.shapeSettingsList.Add(cBodySettings.shape.GetSettings());
            cBodiesSettings.shadingSettingsList.Add(cBodySettings.shading.GetSettings());
            cBodiesSettings.physicsSettingsList.Add(cBodySettings.physics.GetSettings());
        }
        var cBodiesSettingsPath = _storePath + systemSettings.systemName + "_cBodies_settings";
        StreamWriter settingsWriter = new StreamWriter(cBodiesSettingsPath, false);
        var settingsJson = JsonUtility.ToJson(cBodiesSettings);
        settingsWriter.WriteLine(settingsJson);
        settingsWriter.Close();
        
        
        // Stores the system settings
        var systemPath = _storePath + systemSettings.systemName + "_system_settings";
        StreamWriter systemWriter = new StreamWriter(systemPath, false);
        var systemJson = JsonUtility.ToJson(systemSettings);
        systemWriter.WriteLine(systemJson);
        systemWriter.Close();
    }

    public SystemSettings LoadSystem(string systemName)
    {
        var cBodiesTypesPath = _storePath + systemName + "_cBodies_types";
        var cBodiesSettingsPath = _storePath + systemName + "_cBodies_settings";
        var systemPath = _storePath + systemName + "_system_settings";
        
        if (File.Exists(systemPath) && File.Exists(cBodiesTypesPath) && File.Exists(cBodiesSettingsPath))
        {
            // There exists already a previous saved state
            StreamReader cBodiesTypesReader = new StreamReader(cBodiesTypesPath);
            CBodiesTypes loadedCBodiesTypes = 
                JsonUtility.FromJson<CBodiesTypes>(cBodiesTypesReader.ReadToEnd());

            StreamReader cBodiesSettingsReader = new StreamReader(cBodiesSettingsPath);
            CBodiesSettings loadedCBodiesSettings =
                JsonUtility.FromJson<CBodiesSettings>(cBodiesSettingsReader.ReadToEnd());

            StreamReader systemSettingsReader = new StreamReader(systemPath);
            SystemSettings systemSettings = JsonUtility.FromJson<SystemSettings>(systemSettingsReader.ReadToEnd());

            if (loadedCBodiesTypes != null && loadedCBodiesSettings != null && systemSettings != null)
            {
                for (int i = 0; i < loadedCBodiesTypes.types.Length; i ++)
                {
                    (Shape shape, Shading shading, Physics physics) = GetShapeShadingPhysics(loadedCBodiesTypes.types[i]);
                    shape.SetSettings(loadedCBodiesSettings.shapeSettingsList[i]);
                    systemSettings.cBodiesSettings[i].shape = shape;
                    
                    shading.SetSettings(loadedCBodiesSettings.shadingSettingsList[i]);
                    systemSettings.cBodiesSettings[i].shading = shading;
                    
                    physics.SetSettings(loadedCBodiesSettings.physicsSettingsList[i]);
                    systemSettings.cBodiesSettings[i].physics = physics;
                }
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
        SavedSystemsNames savedNames = new SavedSystemsNames();
        if (!File.Exists(systemNamesPath)) return savedNames.savedSysNames;
        StreamReader reader = new StreamReader(systemNamesPath);
        savedNames = JsonUtility.FromJson<SavedSystemsNames>(reader.ReadToEnd());
        return savedNames.savedSysNames;
    }
    
    // Utility class for serializing system cBodies types
    [Serializable]
    private class CBodiesTypes
    {
        public CBodySettings.CBodyType[] types; 
    }

    [Serializable]
    private class CBodiesSettings
    {
        public List<Shape.ShapeSettings> shapeSettingsList = new List<Shape.ShapeSettings>();
        public List<Shading.ShadingSettings> shadingSettingsList = new List<Shading.ShadingSettings>();
        public List<Physics.PhysicsSettings> physicsSettingsList = new List<Physics.PhysicsSettings>();
    }

    // Utility class for serializing systems names
    [Serializable]
    private class SavedSystemsNames
    {
        public List<string> savedSysNames = new List<string>();
    }
}