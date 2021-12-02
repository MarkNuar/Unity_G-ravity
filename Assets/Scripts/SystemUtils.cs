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
    
    private SystemReader _systemReader;
    private SystemWriter _systemWriter;

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

            _systemReader = new SystemReader();
            _systemWriter = new SystemWriter();
            
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
        _systemWriter.SaveSystem(systemSettings, storePath, _jSonSettings);
    }

    public SystemSettings LoadSystem(string systemName)
    {
        return _systemReader.LoadSystem(systemName, storePath, _jSonSettings);
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
    
    
    private class SystemWriter : ISettingsVisitor
    {

        private CBodiesSettings _cBodiesSettings;
        
        public void SaveSystem(SystemSettings systemSettings, string storePath, JsonSerializerSettings settings)
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
            _cBodiesSettings = new CBodiesSettings();
            foreach (CBodySettings cBodySettings in systemSettings.cBodiesSettings)
            {
                cBodySettings.Shape.AcceptVisitor(this);
                cBodySettings.Shading.AcceptVisitor(this);
                cBodySettings.Physics.AcceptVisitor(this);
            }

            var cBodiesSettingsPath = storePath + systemSettings.systemName + "_cBodies_settings.txt";
            StreamWriter settingsWriter = new StreamWriter(cBodiesSettingsPath, false);
            var settingsJson = JsonConvert.SerializeObject(_cBodiesSettings, settings);
            settingsWriter.WriteLine(settingsJson);
            settingsWriter.Close();


            // Stores the system settings
            var systemPath = storePath + systemSettings.systemName + "_system_settings.txt";
            StreamWriter systemWriter = new StreamWriter(systemPath, false);
            var systemJson = JsonConvert.SerializeObject(systemSettings);
            systemWriter.WriteLine(systemJson);
            systemWriter.Close();
        }
        
        public void VisitShapeSettings(RockShape sp)
        {
            _cBodiesSettings.shapeSettingsList.Add(sp.GetSettings());
        }

        public void VisitShapeSettings(GaseousShape sp)
        {
            _cBodiesSettings.shapeSettingsList.Add(sp.GetSettings());
        }

        public void VisitShapeSettings(StarShape sp)
        {
            _cBodiesSettings.shapeSettingsList.Add( sp.GetSettings());
        }

        public void VisitShadingSettings(RockShading sd)
        {
            _cBodiesSettings.shadingSettingsList.Add(sd.GetSettings());
        }

        public void VisitShadingSettings(GaseousShading sd)
        {
            _cBodiesSettings.shadingSettingsList.Add(sd.GetSettings());
        }

        public void VisitShadingSettings(StarShading sd)
        {
            _cBodiesSettings.shadingSettingsList.Add(sd.GetSettings());
        }

        public void VisitPhysicsSettings(Physics ps)
        {
            _cBodiesSettings.physicsSettingsList.Add(ps.GetSettings());
        }

    }
    

    private class SystemReader : ISettingsVisitor
    {
        private CBodiesSettings _loadedCBodiesSettings;

        private SystemSettings _loadedSystemSettings;

        private int _cbIndex;
        
        public SystemSettings LoadSystem(string systemName, string storePath, JsonSerializerSettings settings)
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
                _loadedSystemSettings = JsonConvert.DeserializeObject<SystemSettings>(systemSettingsReader.ReadToEnd());
                
            
                if (loadedCBodiesTypes != null && _loadedSystemSettings != null)
                {
                    _loadedCBodiesSettings = JsonConvert.DeserializeObject<CBodiesSettings>(cBodiesSettingsReader.ReadToEnd(),settings);

                    for (_cbIndex = 0; _cbIndex < loadedCBodiesTypes.types.Length; _cbIndex ++)
                    {
                        (Shape shape, Shading shading, Physics physics) = Instance.GetShapeShadingPhysics(loadedCBodiesTypes.types[_cbIndex]);
 
                        shape.AcceptVisitor(this);
                        shading.AcceptVisitor(this);
                        physics.AcceptVisitor(this);
                    }
                    return _loadedSystemSettings;
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
        
        public void VisitShapeSettings(RockShape sp)
        {
            sp.SetSettings(_loadedCBodiesSettings.shapeSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shape = sp;
        }

        public void VisitShapeSettings(GaseousShape sp)
        {
            sp.SetSettings(_loadedCBodiesSettings.shapeSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shape = sp;
        }

        public void VisitShapeSettings(StarShape sp)
        {
            sp.SetSettings(_loadedCBodiesSettings.shapeSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shape = sp;
        }

        public void VisitShadingSettings(RockShading sd)
        {
            sd.SetSettings(_loadedCBodiesSettings.shadingSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shading = sd;
        }

        public void VisitShadingSettings(GaseousShading sd)
        {
            sd.SetSettings(_loadedCBodiesSettings.shadingSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shading = sd;
        }

        public void VisitShadingSettings(StarShading sd)
        {
            sd.SetSettings(_loadedCBodiesSettings.shadingSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Shading = sd;
        }

        public void VisitPhysicsSettings(Physics ps)
        {
            ps.SetSettings(_loadedCBodiesSettings.physicsSettingsList[_cbIndex]);
            _loadedSystemSettings.cBodiesSettings[_cbIndex].Physics = ps;
        }
    }

}