using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class RockShading : Shading
    {
        public override void RandomInitialize()
        {
            shadingSettings = new RockShadingSettings
            {
                color = Random.ColorHSV()
                // ...
            };
            if(Observer)
                Observer.OnShadingUpdate();
        }
        
        [Serializable]
        public class RockShadingSettings : ShadingSettings
        {
            
        }
    }
}