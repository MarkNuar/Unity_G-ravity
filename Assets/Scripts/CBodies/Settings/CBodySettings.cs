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
        
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia", "Mah", "Craxis"};
        
        public void Init(CBodyType type)
        {
            cBodyName = BaseNames[Random.Range(0, BaseNames.Length)];
            cBodyType = type;
            (Shape.Shape sp, Shading.Shading sd, Physics.Physics ph) = SystemUtils.Instance.GetShapeShadingPhysics(cBodyType);
            shape = sp;
            shading = sd;
            physics = ph;
            
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

        [Serializable]
        public enum CBodyType
        {
            Rocky,
            Gaseous,
            Star
        }
    }
}