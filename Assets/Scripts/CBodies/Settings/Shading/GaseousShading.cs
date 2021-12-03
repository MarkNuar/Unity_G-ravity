using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class GaseousShading : Shading
    {
        public override void RandomInitialize()
        {
            shadingSettings = new GaseousShadingSettings
            {
                color = Random.ColorHSV()
                // ...
            };
            if(Observer)
                Observer.OnShadingUpdate();
        }
        
        [Serializable]
        public class GaseousShadingSettings : ShadingSettings
        {
            
        }

    }
}