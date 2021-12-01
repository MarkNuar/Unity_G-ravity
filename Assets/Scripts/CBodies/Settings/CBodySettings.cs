using System;
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
        
        [NonSerialized] public Shape.Shape Shape;
        [NonSerialized] public Shading.Shading Shading;
        [NonSerialized] public Physics.Physics Physics;
        
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia"};
        
        public void Init(CBodyType type)
        {
            cBodyName = BaseNames[Random.Range(0, BaseNames.Length)];
            cBodyType = type;
            (Shape.Shape sp, Shading.Shading sd, Physics.Physics ph) = SystemUtils.Instance.GetShapeShadingPhysics(cBodyType);
            Shape = sp;
            Shading = sd;
            Physics = ph;
        }

        public void Subscribe(CBodyGenerator observer)
        {
            observer.cBodySettings = this;
            Shape.Subscribe(observer);
            Shading.Subscribe(observer);
            Physics.Subscribe(observer);
        }

        public void Unsubscribe()
        {
            Shape.Unsubscribe();
            Shading.Unsubscribe();
            Physics.Unsubscribe();
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