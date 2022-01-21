using System;
using UnityEngine;
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

        // cosine based palette, 4 vec3 params
        // Inigo Quilez Palette colors
        private Vector3 Palette( in float t, in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d )
        {
            Vector3 r = new Vector3(
                Mathf.Cos(6.28318f * (c.x * t + d.x)),
                Mathf.Cos(6.28318f * (c.y * t + d.y)),
                Mathf.Cos(6.28318f * (c.z * t + d.z))
                );
            Vector3 p = new Vector3(
                b.x * r.x,
                b.y * r.y,
                b.z * r.z
                );
            return a + p;
        }
        
        public override void SetSurfaceProperties(Material material, Vector2 heightMinMax, float bodyScale, float oceanLevel)
        {
            float seed = shadingSettings.seed;
            //Colors:
            // Inigo Quilez color palette
            Vector3 a = new Vector3(0.5f, .25f+(Mathf.Sin(seed*0.1f)-.25f+.25f), 0.5f) * 0.5f;
            Vector3 b = new Vector3(0.5f, 0.5f, 0.5f) * 0.5f;
            Vector3 c = new Vector3(1.0f, 1.0f, 1.0f) * 0.5f;
            Vector3 d = new Vector3(0.0f, 0.33f, 0.67f) * 0.5f;
            
            Vector3 ct =  Palette(seed *  .11f ,  a,  b, c,  d )*.5f;
            Vector3 cb =  Palette(seed *  .115f,  a,  b, c,  d )*.5f;
            Vector3 cm1 = Palette(seed * .1135f,  a,  b, c,  d )*.33f;
            Vector3 cm2 = Palette(seed *-.1135f,  d,  c, b,  a )*.33f;
            Vector3 cm3 = Palette(seed *-.114f ,  c,  d, c,  b )*.33f;

            Color colorTop = new Color(ct.x, ct.y, ct.z);
            Color colorBottom = new Color(cb.x, cb.y, cb.z);
            Color colorMid1 = new Color(cm1.x, cm1.y, cm1.z);
            Color colorMid2 = new Color(cm2.x, cm2.y, cm2.z);
            Color colorMid3 = new Color(cm3.x, cm3.y, cm3.z);
            
            material.SetColor("col_top", colorTop);
            material.SetColor("col_bot", colorBottom);
            material.SetColor("col_mid1", colorMid1);
            material.SetColor("col_mid2", colorMid2);
            material.SetColor("col_mid3", colorMid3);
            
        }

        [Serializable]
        public class GaseousShadingSettings : ShadingSettings
        {

        }
    }
}