using System;
using System.Collections.Generic;
using System.IO;
using CBodies.Settings;
using CBodies.Settings.PostProcessing.Atmosphere;
using CBodies.Settings.PostProcessing.Ocean;
using CBodies.Settings.PostProcessing.Ring;
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

    public SystemSettings currentSystemSettings;
    
    public string storePath = null;
    public string testStorePath = null;

    private JsonSerializerSettings _jSonSettings;

    [Tooltip("Decide whether to instantiate a copy of the settings, or to use (and edit) them directly")] 
    public bool createCopyOfSettings = true;
    
    // SHAPES 
    [Header("Shapes")] 
    public MoonShape moonShape;
    public PlanetShape planetShape;
    public GaseousShape gaseousShape;
    public StarShape starShape;

    // SHADING 
    [Header("Shading")] 
    public MoonShading moonShading;
    public PlanetShading planetShading;
    public GaseousShading gaseousShading;
    public StarShading starShading;
    
    [Header("Physics")] 
    public Physics moonPhysics;
    public Physics planetPhysics;
    public Physics gaseousPhysics;
    public Physics starPhysics;
    
    // OCEAN
    [Header("Ocean")] 
    public Ocean baseOcean;
    
    // ATMOSPHERE
    [Header("Atmosphere")] 
    public Atmosphere baseAtmosphere;
    
    // RING 
    [Header("Ring")] 
    public Ring baseRing;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(Instance);

            // Save the store path
            storePath = Application.persistentDataPath + Path.DirectorySeparatorChar;
            testStorePath = storePath + "test" + Path.DirectorySeparatorChar;
            
            // Setting the de/-serialization settings 
            _jSonSettings = new JsonSerializerSettings();
            _jSonSettings.ConstructorHandling = ConstructorHandling.Default;
            _jSonSettings.Converters.Add(JsonSubtypesConverterBuilder
                .Of<Shape.ShapeSettings>("Type") // type property is only defined here
                .RegisterSubtype<PlanetShape.PlanetShapeSettings>(CBodySettings.CBodyType.Planet)
                .RegisterSubtype<GaseousShape.GaseousShapeSettings>(CBodySettings.CBodyType.Gaseous)
                .RegisterSubtype<StarShape.StarShapeSettings>(CBodySettings.CBodyType.Star)
                .RegisterSubtype<MoonShape.MoonShapeSettings>(CBodySettings.CBodyType.Moon)
                .SerializeDiscriminatorProperty() // ask to serialize the type property
                .Build());
            _jSonSettings.Converters.Add(JsonSubtypesConverterBuilder
                .Of<Shading.ShadingSettings>("Type") // type property is only defined here
                .RegisterSubtype<PlanetShading.PlanetShadingSettings>(CBodySettings.CBodyType.Planet)
                .RegisterSubtype<GaseousShading.GaseousShadingSettings>(CBodySettings.CBodyType.Gaseous)
                .RegisterSubtype<StarShading.StarShadingSettings>(CBodySettings.CBodyType.Star)
                .RegisterSubtype<MoonShading.MoonShadingSettings>(CBodySettings.CBodyType.Moon)
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


        // // Stores the types of the newly created system cbodies
        // CBodiesTypes cBodiesTypes = new CBodiesTypes
        // {
        //     types = new CBodySettings.CBodyType[systemSettings.cBodiesSettings.Count]
        // };
        // for (var i = 0; i < systemSettings.cBodiesSettings.Count; i++)
        // {
        //     cBodiesTypes.types[i] = systemSettings.cBodiesSettings[i].cBodyType;
        // }
        //
        // var cBodiesTypesPath = storePath + systemSettings.systemName + "_cBodies_types.txt";
        // StreamWriter typesWriter = new StreamWriter(cBodiesTypesPath, false);
        // var typesJson = JsonConvert.SerializeObject(cBodiesTypes);
        // typesWriter.WriteLine(typesJson);
        // typesWriter.Close();


        // Stores cBody settings for each cBody
        CBodiesSettings toStoreCBodiesSettings = new CBodiesSettings();
        foreach (CBodySettings cBodySettings in systemSettings.cBodiesSettings)
        {
            toStoreCBodiesSettings.shapeSettingsList.Add(cBodySettings.shape.GetSettings());
            toStoreCBodiesSettings.shadingSettingsList.Add(cBodySettings.shading.GetSettings());
            toStoreCBodiesSettings.physicsSettingsList.Add(cBodySettings.physics.GetSettings());
            toStoreCBodiesSettings.oceanSettingsList.Add(cBodySettings.ocean.GetSettings());
            toStoreCBodiesSettings.atmosphereSettingsList.Add(cBodySettings.atmosphere.GetSettings());
            toStoreCBodiesSettings.ringSettingsList.Add(cBodySettings.ring.GetSettings());
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
        if (!Directory.Exists(testStorePath))
        {
            Directory.CreateDirectory(testStorePath);
        }
        
        var temp = storePath;
        storePath = testStorePath;
        SaveSystem(systemSettings);
        storePath = temp;
    }

    public SystemSettings LoadSystem(string systemName)
    {
        // var cBodiesTypesPath = storePath + systemName + "_cBodies_types.txt";
        var cBodiesSettingsPath = storePath + systemName + "_cBodies_settings.txt";
        var systemPath = storePath + systemName + "_system_settings.txt";
        
        // if (File.Exists(systemPath) && File.Exists(cBodiesTypesPath) && File.Exists(cBodiesSettingsPath))
        if (File.Exists(systemPath) && File.Exists(cBodiesSettingsPath))
        {
            // There exists already a previous saved state
            // StreamReader cBodiesTypesReader = new StreamReader(cBodiesTypesPath);
            // CBodiesTypes loadedCBodiesTypes = JsonConvert.DeserializeObject<CBodiesTypes>(cBodiesTypesReader.ReadToEnd());
            
            StreamReader cBodiesSettingsReader = new StreamReader(cBodiesSettingsPath);

            StreamReader systemSettingsReader = new StreamReader(systemPath);
            SystemSettings loadedSystemSettings = JsonConvert.DeserializeObject<SystemSettings>(systemSettingsReader.ReadToEnd());
            
        
            // if (loadedCBodiesTypes != null && loadedSystemSettings != null)
            if (loadedSystemSettings != null)
            {
                CBodiesSettings loadedCBodiesSettings = JsonConvert.DeserializeObject<CBodiesSettings>(cBodiesSettingsReader.ReadToEnd(),_jSonSettings);

                for (int i = 0; i < loadedSystemSettings.cBodiesSettings.Count; i ++)
                {
                    (Shape shape, Shading shading, Physics physics, Ocean ocean, Atmosphere atmosphere, Ring ring) = Instance.GetSettings(loadedSystemSettings.cBodiesSettings[i].cBodyType);
                    
                    shape.SetSettings(loadedCBodiesSettings.shapeSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].shape = shape;
                    shading.SetSettings(loadedCBodiesSettings.shadingSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].shading = shading;
                    physics.SetSettings(loadedCBodiesSettings.physicsSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].physics = physics;
                    ocean.SetSettings(loadedCBodiesSettings.oceanSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].ocean = ocean;
                    atmosphere.SetSettings(loadedCBodiesSettings.atmosphereSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].atmosphere = atmosphere;
                    ring.SetSettings(loadedCBodiesSettings.ringSettingsList[i]);
                    loadedSystemSettings.cBodiesSettings[i].ring = ring;
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

    public (Shape shape, Shading shading, Physics physics, Ocean ocean, Atmosphere atmosphere, Ring ring) GetSettings(CBodySettings.CBodyType type)
    {
        // todo return atmosphere too
        Shape shape = null;
        Shading shading = null;
        Physics physics = null;
        Ocean ocean = null;
        Atmosphere atmosphere = null;
        Ring ring = null;
        
        if (createCopyOfSettings)
        {
            ocean = Instantiate(baseOcean);
            atmosphere = Instantiate(baseAtmosphere);
            ring = Instantiate(baseRing);
            
            switch (type)
            {
                case CBodySettings.CBodyType.Moon:
                    shape = Instantiate(moonShape);
                    shading = Instantiate(moonShading);
                    physics = Instantiate(moonPhysics);
                    break;
                case CBodySettings.CBodyType.Planet:
                    shape = Instantiate(planetShape);
                    shading = Instantiate(planetShading);
                    physics = Instantiate(planetPhysics);
                    break;
                case CBodySettings.CBodyType.Gaseous:
                    shape = Instantiate(gaseousShape);
                    shading = Instantiate(gaseousShading);
                    physics = Instantiate(gaseousPhysics);
                    break;
                case CBodySettings.CBodyType.Star:
                    shape = Instantiate(starShape);
                    shading = Instantiate(starShading);
                    physics = Instantiate(starPhysics);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        else
        {
            ocean = (baseOcean);
            atmosphere = (baseAtmosphere);
            ring = (baseRing);
            
            switch (type)
            {
                case CBodySettings.CBodyType.Moon:
                    shape = (moonShape);
                    shading = (moonShading);
                    physics = (moonPhysics);
                    break;
                case CBodySettings.CBodyType.Planet:
                    shape = (planetShape);
                    shading = (planetShading);
                    physics = planetPhysics;
                    break;
                case CBodySettings.CBodyType.Gaseous:
                    shape = (gaseousShape);
                    shading = (gaseousShading);
                    physics = gaseousPhysics;
                    break;
                case CBodySettings.CBodyType.Star:
                    shape = (starShape);
                    shading = (starShading);
                    physics = starPhysics;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        

        return (shape, shading, physics, ocean, atmosphere, ring);
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
        public List<Ocean.OceanSettings> oceanSettingsList = new List<Ocean.OceanSettings>();
        public List<Atmosphere.AtmosphereSettings> atmosphereSettingsList = new List<Atmosphere.AtmosphereSettings>();
        public List<Ring.RingSettings> ringSettingsList = new List<Ring.RingSettings>();
    }

    // Utility class for serializing systems names
    [Serializable]
    private class SavedSystemsNames
    {
        public List<string> savedSysNames = new List<string>();
    }
}