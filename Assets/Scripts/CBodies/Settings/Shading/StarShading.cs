using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class StarShading : Shading
    {
        // MEMENTO
        [SerializeReference] protected StarShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shadingSettings = new StarShadingSettings
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
            shadingSettings = (StarShadingSettings)ss;
            if(Observer)
                Observer.OnShadingUpdate();
        }

        [Serializable]
        public class StarShadingSettings : ShadingSettings
        {
            
        }
    }
}