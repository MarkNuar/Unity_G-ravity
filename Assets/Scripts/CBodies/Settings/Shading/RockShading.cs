using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class RockShading : Shading
    {
        // MEMENTO
        [SerializeReference] protected RockShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
            shadingSettings = new RockShadingSettings
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
            shadingSettings = (RockShadingSettings)ss;
            if(Observer)
                Observer.OnShadingUpdate();
        }

        [Serializable]
        public class RockShadingSettings : ShadingSettings
        {
            
        }
    }
}