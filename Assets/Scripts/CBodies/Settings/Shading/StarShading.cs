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
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShadingUpdate();
            }
        }
        public override ShadingSettings GetSettings()
        {
            return shadingSettings;
        }
        public override void SetSettings(ShadingSettings ss)
        {
            shadingSettings = (StarShadingSettings)ss;
            
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShadingUpdate();
            }
        }

        [Serializable]
        public class StarShadingSettings : ShadingSettings
        {
            
        }
    }
}