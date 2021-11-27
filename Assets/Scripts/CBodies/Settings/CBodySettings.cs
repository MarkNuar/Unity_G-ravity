using System;
using CBodies.Settings.Physics;
using CBodies.Settings.Shading;
using CBodies.Settings.Shape;
using Random = UnityEngine.Random;

namespace CBodies.CBodySettings
{
    [Serializable]
    public class CBodySettings
    {
        private static readonly string[] BaseNames = {"Plutu", "Merci", "Nanastria", "Regemonia"};

        public string name;

        //public AppearanceData appearance = new AppearanceData();
        public Shape shape = new Shape();
        public Shading shading = new Shading();
        public Physics physics = new Physics();

        public void Init()
        {
            name = BaseNames[Random.Range(0, BaseNames.Length)];
            //
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
    }
}