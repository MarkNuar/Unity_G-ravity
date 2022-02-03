using System;
using UnityEngine;
using Utilities;

namespace CBodies.Settings.PostProcessing.Ring
{
    [Serializable][CreateAssetMenu]
    public class Ring : ScriptableObject 
    {
        private static System.Random _prng = new System.Random ();
        
        // MEMENTO
        [SerializeField] private RingSettings ringSettings;
        
        public bool hasPhysicChanged;
        
        public void SetRingProperties(Material ringMaterial)
        {
            ringMaterial.SetInt("has_ring", ringSettings.hasRing ? 1 : 0);
            if (ringSettings.hasRing)
            {
                // do not check if randomize color, the base color is decided by the palette, given a color seed equal to 0
                PRNG randomColor = new PRNG(ringSettings.shadingSeed);
                PRNG randomShape = new PRNG(ringSettings.shapeSeed);
                Vector3 a = new Vector3(0.5f, .25f+(Mathf.Sin(randomColor.Range(0.0f,20.0f)*.1f))-.25f+.25f, 0.5f);
                Vector3 b = new Vector3(0.1f, 0.1f, 0.1f);
                Vector3 c = new Vector3(1.0f, 1.0f, 1.0f);
                Vector3 d = new Vector3(0.0f, 0.33f, 0.64f);
                
                Color ringColor = ColorHelper.Palette( randomColor.Range(0.0f,20.0f) * .1135f, a,  b, c,  d );
                
                ringMaterial.SetColor("ring_color", ringColor);
                ringMaterial.SetFloat("seed", randomShape.Range(0.0f, 1.0f));
                
                
                if (ringSettings.randomizeShape)
                {
                    var innerRadiusPercent = 1.05f + randomShape.Range(0.0f, 0.4f);
                    var outerRadiusPercent = innerRadiusPercent + randomShape.Range(0.2f, 0.5f);

                    var yNormal = 1 - randomShape.Range(0.0f, 0.4f);
                    var xNormal = 0.1f + randomShape.Range(-0.2f, 0.2f);
                    var zNormal = 0.2f + randomShape.Range(0.0f, 0.3f);
                
                    Vector3 ringNormal = new Vector3(xNormal, yNormal, zNormal);
                
                    ringMaterial.SetFloat("inner_radius_percent", innerRadiusPercent);
                    ringMaterial.SetFloat("outer_radius_percent", outerRadiusPercent);
                    ringMaterial.SetVector("ring_normal", ringNormal.normalized);
                    
                }
                else
                {
                    ringMaterial.SetFloat("inner_radius_percent", ringSettings.baseInnerRadiusPercent);
                    ringMaterial.SetFloat("outer_radius_percent", ringSettings.baseOuterRadiusPercent);
                    ringMaterial.SetVector("ring_normal", ringSettings.baseRingNormal.normalized);
                }
            }
        }

        [Serializable]
        public class RingSettings
        {
            // todo ring color
            
            public bool randomizeShading;
            public bool randomizeShape;
            public int shadingSeed;
            public int shapeSeed;

            public bool hasRing;

            // ring radius percent with respect to planet radius
            // these values must be greater than 1
            [Range(1.1f, 10)] public float baseInnerRadiusPercent = 1.5f;
            [Range(1.1f, 10)] public float baseOuterRadiusPercent = 3.4f; 
            
            // serialize plane normal inclination, quaternion, vector 3, angles?
            public Vector3 baseRingNormal = new Vector3(0.0f, 1.0f, 0.2f);
            
            public void RandomizeShading(bool rand)
            {
                randomizeShading = rand;
                shadingSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
            
            public void RandomizeShape(bool rand)
            {
                randomizeShape = rand;
                shapeSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
        }
        
        // MEMENTO PATTERN
        public void InitSettings()
        {

        }
        public RingSettings GetSettings()
        {
            return ringSettings;
        }

        public void SetSettings(RingSettings rs)
        {
            ringSettings = rs;
        }
    }
}