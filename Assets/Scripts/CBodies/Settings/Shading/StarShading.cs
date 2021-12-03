using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class StarShading : Shading
    {
        public override void RandomInitialize()
        {
            shadingSettings = new StarShadingSettings
            {
                color = Random.ColorHSV()
                // ...
            };
            if(Observer)
                Observer.OnShadingUpdate();
        }

        [Serializable]
        public class StarShadingSettings : ShadingSettings
        {
            
        }
    }
}