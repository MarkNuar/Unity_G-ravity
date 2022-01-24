using System;
using UnityEngine;
using Utilities;

namespace CBodies.Settings.PostProcessingSettings.Ring
{
    [Serializable][CreateAssetMenu]
    public class Ring : ScriptableObject 
    {
        private static System.Random _prng = new System.Random ();
        
        // MEMENTO
        [SerializeField] private RingSettings ringSettings;
        
        public void SetRingProperties(Material ringMaterial)
        {
            ringMaterial.SetInt("has_ring", ringSettings.hasRing ? 1 : 0);
            if (ringSettings.hasRing)
            {
                if (ringSettings.randomize)
                {
                    int normalSeed = ringSettings.ringNormalSeed;
                    int radiusSeed = ringSettings.ringRadiusSeed;
                
                    var innerRadiusPercent = 1.1f + ColorHelper.Map(radiusSeed, 0, 10000, 0, 0.4f);
                    var outerRadiusPercent = innerRadiusPercent + ColorHelper.Map(radiusSeed, 0, 10000, 0, 0.5f) + 0.05f;

                    var yNormal = 1 - ColorHelper.Map(normalSeed, 0, 10000, 0, 0.4f);
                    var xNormal = 0.1f + ColorHelper.Map(normalSeed, 0, 10000, -0.2f, 0.2f);
                    var zNormal = 0.2f + ColorHelper.Map(normalSeed, 0, 10000, 0, 0.3f);
                
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
            public bool randomize;
            public int ringNormalSeed;
            public int ringRadiusSeed;
            
            public bool hasRing;

            // ring radius percent with respect to planet radius
            // these values must be greater than 1
            [Range(1.1f, 10)] public float baseInnerRadiusPercent = 1.5f;
            [Range(1.1f, 10)] public float baseOuterRadiusPercent = 3.4f; 
            
            // serialize plane normal inclination, quaternion, vector 3, angles?
            public Vector3 baseRingNormal = new Vector3(0.0f, 1.0f, 0.2f);
            
            public void UpdateSeed(bool rand)
            {
                ringNormalSeed = rand ? _prng.Next(1, 10000) : 0;
                ringRadiusSeed = rand ? _prng.Next(1, 10000) : 0;
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