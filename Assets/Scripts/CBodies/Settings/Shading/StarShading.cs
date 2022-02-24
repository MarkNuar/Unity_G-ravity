using System;
using UnityEngine;
using Utilities;
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
                var deltaH = random.Range(-0.05f, 0.05f);
                switch (n)
                {
                    case 0: surfaceColor = ColorHelper.TweakHSV(shadingSettings.yellow, deltaH, 0,0);
                        shadingSettings.mainColor = Color.yellow;
                        break;
                    case 1: surfaceColor = ColorHelper.TweakHSV(shadingSettings.blue, deltaH, 0,0);
                        shadingSettings.mainColor = Color.blue;
                        break;
                    case 2: surfaceColor = ColorHelper.TweakHSV(shadingSettings.red, deltaH, 0,0);
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