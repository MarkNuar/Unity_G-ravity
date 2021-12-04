using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class GaseousShading : Shading
    {
        // MEMENTO
        [SerializeReference] protected GaseousShadingSettings shadingSettings;
        
        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shadingSettings = new GaseousShadingSettings
            {
                color = Random.ColorHSV()
            };
            if(Observer)
                Observer.OnShadingUpdate();
        }
        public override ShadingSettings GetSettings()
        {
            return shadingSettings;
        }

        public override void SetSettings(ShadingSettings ss)
        {
            shadingSettings = (GaseousShadingSettings)ss;
            if(Observer)
                Observer.OnShadingUpdate();
        }

        [Serializable]
        public class GaseousShadingSettings : ShadingSettings
        {
            
        }

    }
}