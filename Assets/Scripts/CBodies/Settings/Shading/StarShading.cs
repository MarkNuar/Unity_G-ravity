using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class StarShading : Shading
    {
        // MEMENTO
        [SerializeField] protected StarShadingSettings shadingSettings;

        // MEMENTO PATTERN
        public override void InitSettings()
        {
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