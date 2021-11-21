using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class CBodyData
    {
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia"};

        public string name;

        //public AppearanceData appearance = new AppearanceData();
        public ShapeData shapeData = new ShapeData();
        public ShadingData shadingData = new ShadingData();
        public PhysicsData physicsData = new PhysicsData();
        public CBodyType cBodyType;

        public void Init()
        {
            // TODO
            cBodyType = CBodyType.Rocky; // Default
            
            name = BaseNames[Random.Range(0, BaseNames.Length)];
            //
        }

        public void Subscribe(CBody observer)
        {
            shapeData.Subscribe(observer);
            shadingData.Subscribe(observer);
            physicsData.Subscribe(observer);
        }

        public void Unsubscribe()
        {
            shapeData.Unsubscribe();
            shadingData.Unsubscribe();
            physicsData.Unsubscribe();
        }

        public enum CBodyType
        {
            Rocky,
            Gaseous,
            Star
        }
    }
}