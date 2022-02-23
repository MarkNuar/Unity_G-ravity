using System;
using UnityEngine;
using Utilities;
using Random = UnityEngine.Random;

namespace CBodies.Settings.Shading
{
    [Serializable][CreateAssetMenu]
    public class GaseousShading : Shading
    {
        // MEMENTO
        [SerializeField] protected GaseousShadingSettings shadingSettings;
        
        // MEMENTO PATTERN
        public override void InitSettings()
        {
            SetRandomColors();
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
            shadingSettings = (GaseousShadingSettings)ss;
            
            if (Observers == null) return;
            foreach (ICBodyObserver o in Observers)
            {
                o.OnShadingUpdate();
            }
        }

        public override void SetSurfaceProperties(Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel, bool hasAtmosphere)
        {
            SetRandomColors();
            
            material.SetColor("col_top", shadingSettings.colors.colorTop);
            material.SetColor("col_bot", shadingSettings.colors.colorBottom);
            material.SetColor("col_mid1", shadingSettings.colors.colorMid1);
            material.SetColor("col_mid2", shadingSettings.colors.colorMid2);
            material.SetColor("col_mid3", shadingSettings.colors.colorMid3);
            
            material.SetFloat("random_val", shadingSettings.randomVal);
        }

        private void SetRandomColors()
        {
            int seed = shadingSettings.seed;
            PRNG random = new PRNG(seed);
            shadingSettings.randomVal = random.Range(0.0f, 20.0f);
            
            Vector3 a = new Vector3(0.5f, .25f+(Mathf.Sin(shadingSettings.randomVal*.1f)-.25f+.25f), 0.5f);
            Vector3 b = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 c = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 d = new Vector3(0.0f, 0.33f, 0.64f);
            
            shadingSettings.colors.colorTop = Color.white;
            shadingSettings.colors.colorBottom = Color.black;
            shadingSettings.colors.colorMid1 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( shadingSettings.randomVal * .1135f, a,  b, c,  d ) * 0.33f;//, 0, 0.3f, 0);
            shadingSettings.colors.colorMid2 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( shadingSettings.randomVal * -.1135f, d,  c, b,  a ) * 0.33f;//, 0, 0.3f, 0);
            shadingSettings.colors.colorMid3 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( shadingSettings.randomVal * -.114f, c,  d, c,  b ) * 0.33f;//, 0, 0.3f, 0);

            shadingSettings.mainColor = shadingSettings.colors.colorMid1;
        }

        [Serializable]
        public class GaseousShadingSettings : ShadingSettings
        {
            public GaseousColors colors;
            public float randomVal;
        }
        
        [Serializable]
        public struct GaseousColors
        {
            public Color colorTop;
            public Color colorBottom;
            public Color colorMid1;
            public Color colorMid2;
            public Color colorMid3;
        }
    }
}