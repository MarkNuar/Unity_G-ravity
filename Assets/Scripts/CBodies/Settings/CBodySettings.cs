using System;
using Newtonsoft.Json;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings
{
    [Serializable]
    public class CBodySettings
    {
        // ***** SERIALIZED
        public string cBodyName; 
        public CBodyType cBodyType;
        // ***** END
        
        [JsonIgnore] public Shape.Shape shape;
        [JsonIgnore] public Shading.Shading shading; 
        [JsonIgnore] public Physics.Physics physics;
        [JsonIgnore] public Ocean.Ocean ocean;
        
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia", "Mah", "Craxis"};
        
        public void Init(CBodyType type)
        {
            cBodyName = BaseNames[Random.Range(0, BaseNames.Length)];
            UpdateCBodyType(type);
        }

        public void Subscribe(CBodyGenerator observer)
        {
            observer.cBodySettings = this;
            shape.Subscribe(observer);
            shading.Subscribe(observer);
            physics.Subscribe(observer);
            
            observer.OnInitialUpdate();
        }

        public void Unsubscribe()
        {
            shape.Unsubscribe();
            shading.Unsubscribe();
            physics.Unsubscribe();
        }

        public void UpdateCBodyType(CBodyType newType)
        {
            cBodyType = newType;
            (Shape.Shape sp, Shading.Shading sd, Physics.Physics ph, Ocean.Ocean oc) = SystemUtils.Instance.GetShapeShadingPhysics(cBodyType);
            shape = sp;
            shading = sd;
            physics = ph;
            ocean = oc;
            sp.InitSettings();
            sd.InitSettings();
            ph.InitSettings();
            oc.InitSettings();

            // Enable ocean by default only on rocky planets
            Ocean.Ocean.OceanSettings os = ocean.GetSettings();
            os.hasOcean = cBodyType == CBodyType.Rocky;
            
        }

        [Serializable]
        public enum CBodyType
        {
            Rocky,
            Gaseous,
            Star
        }
    }
}