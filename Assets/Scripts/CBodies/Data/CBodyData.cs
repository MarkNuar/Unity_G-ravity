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
        public MeshData meshData = new MeshData();
        public MaterialData materialData = new MaterialData();
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
            meshData.Subscribe(observer);
            materialData.Subscribe(observer);
            physicsData.Subscribe(observer);
        }

        public void Unsubscribe()
        {
            meshData.Unsubscribe();
            materialData.Unsubscribe();
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