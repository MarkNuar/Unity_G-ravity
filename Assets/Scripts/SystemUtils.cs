using System;
using System.Collections.Generic;
using System.IO;
using CBodies.Settings;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using JsonSubTypes;
using Newtonsoft.Json;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

// SINGLETON
public class SystemUtils : MonoBehaviour
{
    public static SystemUtils Instance;

    public string storePath = null;
    public string testStorePath = null;

    private JsonSerializerSettings _jSonSettings;
    
    // SHAPES 
    [Header("Shapes")] public RockShape rockShape;
    public GaseousShape gaseousShape;
    public StarShape starShape;

    // SHADING 
    [Header("Shading")] public RockShading rockShading;
    public GaseousShading gaseousShading;
    public StarShading starShading;

    // PHYSICS
    [Header("Physics")] public Physics basePhysics;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);

            // Save the store path
            storePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
            testStorePath = storePath + "test" + Path.DirectorySeparatorChar;
            Debug.Log(storePath);
            Debug.Log(testStorePath);

            // Setting the de/-serialization settings 
            _jSonSettings = new JsonSerializerSettings();
            _jSonSettings.Converters.Add(JsonSubtypesConverterBuilder
                .Of<Shape.ShapeSettings>("Type") // type property is only defined here
                .RegisterSubtype<RockShape.RockShapeSettings>(CBodySettings.CBodyType.Rocky)
                .RegisterSubtype<GaseousShape.GaseousShapeSettings>(CBodySettings.CBodyType.Gaseous)
                .RegisterSubtype<StarShape.StarShapeSettings>(CBodySettings.CBodyType.Star)
                .SerializeDiscriminatorProperty() // ask to serialize the type property
                .Build());
            _jSonSettings.Converters.Add(JsonSubtypesConverterBuilder
                .Of<Shading.ShadingSettings>("Type") // type property is only defined here
                .RegisterSubtype<RockShading.RockShadingSettings>(CBodySettings.CBodyType.Rocky)
                .RegisterSubtype<GaseousShading.GaseousShadingSettings>(CBodySettings.CBodyType.Gaseous)
                .RegisterSubtype<StarShading.StarShadingSettings>(CBodySettings.CBodyType.Star)
                .SerializeDiscriminatorProperty() // ask to serialize the type property
                .Build());
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveSystem(SystemSettings systemSettings)
    {
        if (storePath == null) return;

        // Add the newly created system name to the system names list
        var systemNamesPath = storePath + "names_of_systems.txt";
        SavedSystemsNames savedNames = new SavedSystemsNames();
        if (File.Exists(systemNamesPath))
        {
            StreamReader namesReader = new StreamReader(systemNamesPath);
            savedNames = JsonConvert.DeserializeObject<SavedSystemsNames>(namesReader.ReadToEnd());
            namesReader.Close();
        }

        if (!savedNames.savedSysNames.Contains(systemSettings.systemName))
        {
            savedNames.savedSysNames.Add(systemSettings.systemName);
        }

        StreamWriter namesWriter = new StreamWriter(systemNamesPath, false);
        var namesJson = JsonConvert.SerializeObject(savedNames);
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

        var cBodiesTypesPath = storePath + systemSettings.systemName + "_cBodies_types.txt";
        StreamWriter typesWriter = new StreamWriter(cBodiesTypesPath, false);
        var typesJson = JsonConvert.SerializeObject(cBodiesTypes);
        typesWriter.WriteLine(typesJson);
        typesWriter.Close();


        // Stores cBody settings for each cBody
        CBodiesSettings toStoreCBodiesSettings = new CBodiesSettings();
        foreach (CBodySettings cBodySettings in systemSettings.cBodiesSettings)
        {

            toStoreCBodiesSettings.shapeSettingsList.Add(cBodySettings.shape.GetSettings());
            toStoreCBodiesSettings.shadingSettingsList.Add(cBodySettings.shading.GetSettings());
            toStoreCBodiesSettings.physicsSettingsList.Add(cBodySettings.physics.GetSettings());
        }

        var cBodiesSettingsPath = storePath + systemSettings.systemName + "_cBodies_settings.txt";
        StreamWriter settingsWriter = new StreamWriter(cBodiesSettingsPath, false);
        var settingsJson = JsonConvert.SerializeObject(toStoreCBodiesSettings, _jSonSettings);
        settingsWriter.WriteLine(settingsJson);
        settingsWriter.Close();


        // Stores the system settings
        var systemPath = storePath + systemSettings.systemName + "_system_settings.txt";
        StreamWriter systemWriter = new StreamWriter(systemPath, false);
        var systemJson = JsonConvert.SerializeObject(systemSettings);
        systemWriter.WriteLine(systemJson);
        systemWriter.Close();
    }

    public void SaveTestSystem(SystemSettings systemSettings)
    {
        var temp = storePath;
        storePath = testStorePath;
        SaveSystem(systemSettings);
        storePath = temp;
    }

    public SystemSettings LoadSystem(string systemName)
    {
        var cBodiesTypesPath = storePath + systemName + "_cBodies_types.txt";
        var cBodiesSettingsPath = storePath + systemName + "_cBodies_settings.txt";
        var systemPath = storePath + systemName + "_system_settings.txt";
        
        if (File.Exists(systemPath) && File.Exists(cBodiesTypesPath) && File.Exists(cBodiesSettingsPath))
        {
            // There exists already a previous saved state
            StreamReader cBodiesTypesReader = new StreamReader(cBodiesTypesPath);
            CBodiesTypes loadedCBodiesTypes = JsonConvert.DeserializeObject<CBodiesTypes>(cBodiesTypesReader.ReadToEnd());
            
            StreamReader cBodiesSettingsReader = new StreamReader(cBodiesSettingsPath);

            StreamReader systemSettingsReader = new StreamReader(systemPath);
            SystemSettings loadedSystemSettings = JsonConvert.DeserializeObject<SystemSettings>(systemSettingsReader.ReadToEnd());
            
        
            if (loadedCBodiesTypes != null && loadedSystemSettings != null)
            {
                CBodiesSettings loadedCBodiesSettings = JsonConvert.DeserializeObject<CBodiesSettings>(cBodiesSettingsReader.ReadToEnd(),_jSonSettings);

                for (int i = 0; i < loadedCBodiesTypes.types.Length; i ++)
                {
                    (Shape shape, Shading shading, Physics physics) = Instance.GetShapeShadingPhysics(loadedCBodiesTypes.types[i]);
                    
                    shape.SetSettings(loadedCBodiesSettings.shapeSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].shape = shape;
                    shading.SetSettings(loadedCBodiesSettings.shadingSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].shading = shading;
                    physics.SetSettings(loadedCBodiesSettings.physicsSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].physics = physics;
                }
                return loadedSystemSettings;
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

    public SystemSettings LoadTestSystem(string systemName)
    {
        var temp = storePath;
        storePath = testStorePath;
        SystemSettings ss = LoadSystem(systemName);
        storePath = temp;
        return ss;
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
        var systemNamesPath = storePath + "names_of_systems.txt";
        SavedSystemsNames savedNames = new SavedSystemsNames();
        if (!File.Exists(systemNamesPath)) return savedNames.savedSysNames;
        StreamReader reader = new StreamReader(systemNamesPath);
        savedNames = JsonConvert.DeserializeObject<SavedSystemsNames>(reader.ReadToEnd());
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