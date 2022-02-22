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

        public override void SetSurfaceProperties(Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel)
        {
            Color surfaceColor = Color.black; 
            if (shadingSettings.randomize)
            {
                PRNG random = new PRNG (shadingSettings.seed);
                var n = random.Range(0, 3);
                switch (n)
                {
                    case 0: surfaceColor = shadingSettings.yellow;
                        shadingSettings.mainColor = Color.yellow;
                        break;
                    case 1: surfaceColor = shadingSettings.blue;
                        shadingSettings.mainColor = Color.blue;
                        break;
                    case 2: surfaceColor = shadingSettings.red;
                        shadingSettings.mainColor = Color.red;
                        break;
                }
            }
            else
            {
                surfaceColor = shadingSettings.yellow;
            }
            
            material.SetColor("_EmissionColor", surfaceColor * 4);
        }

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
            public Color yellow;
            public Color blue;
            public Color red;
        }
    }
}