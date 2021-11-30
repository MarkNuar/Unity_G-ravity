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
        
        public Shape.Shape shape;
        public Shading.Shading shading;
        public Physics.Physics physics;
        
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia"};
        
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
            Base,
            Rocky,
            Gaseous,
            Star
        }
    }
}