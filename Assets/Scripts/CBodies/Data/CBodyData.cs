using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace CBodies.Data
{
    [Serializable]
    public class CBodyData
    {
        private static readonly string[] BaseNames = {"Plutu","Merci","Nanastria","Regemonia"};
        
        public string name;
        public AppearanceData appearance = new AppearanceData();
        public PhysicsData physics = new PhysicsData();

        public void Init()
        {
            name = BaseNames[Random.Range(0, BaseNames.Length)];
            //
        }

        public void Subscribe(CBody observer)
        {
            appearance.Subscribe(observer);
            physics.Subscribe(observer);
        }

        public void Unsubscribe()
        {
            appearance.Unsubscribe();
            physics.Unsubscribe();
        }
    }
}