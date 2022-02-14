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

        public override void SetSurfaceProperties(Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel)
        {
            int seed = shadingSettings.seed;
            PRNG random = new PRNG(seed);
            float randomVal = random.Range(0.0f, 20.0f);
            
            Vector3 a = new Vector3(0.5f, .25f+(Mathf.Sin(randomVal*.1f)-.25f+.25f), 0.5f);
            Vector3 b = new Vector3(0.5f, 0.5f, 0.5f);
            Vector3 c = new Vector3(1.0f, 1.0f, 1.0f);
            Vector3 d = new Vector3(0.0f, 0.33f, 0.64f);
            
            Color colorTop = Color.white;
            Color colorBottom = Color.black;
            Color colorMid1 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( randomVal * .1135f, a,  b, c,  d ) * 0.33f;//, 0, 0.3f, 0);
            Color colorMid2 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( randomVal * -.1135f, d,  c, b,  a ) * 0.33f;//, 0, 0.3f, 0);
            Color colorMid3 = //ColorHelper.TweakHSV(
                ColorHelper.Palette( randomVal * -.114f, c,  d, c,  b ) * 0.33f;//, 0, 0.3f, 0);

            material.SetColor("col_top", colorTop);
            material.SetColor("col_bot", colorBottom);
            material.SetColor("col_mid1", colorMid1);
            material.SetColor("col_mid2", colorMid2);
            material.SetColor("col_mid3", colorMid3);
            
            material.SetFloat("random_val", randomVal);
        }

        [Serializable]
        public class GaseousShadingSettings : ShadingSettings
        {
            
        }
    }
}