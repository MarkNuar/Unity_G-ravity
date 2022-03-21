using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities;

namespace CBodies.Settings.PostProcessing.Atmosphere
{
    [Serializable][CreateAssetMenu]
    public class Atmosphere : ScriptableObject
    {
        private static System.Random _prng = new System.Random ();

        // MEMENTO
        [SerializeField] private AtmosphereSettings atmosphereSettings;
        
        public ComputeShader opticalDepthCompute;
        
        private RenderTexture _opticalDepthTexture;

        private bool _settingsUpToDate = false;

        public void SetAtmosphereProperties(Material material, float cBodyRadius)
        {
            // radius must always be checked
            var atmosphereRadius = (1 + atmosphereSettings.atmosphereScale) * cBodyRadius;
            material.SetFloat ("atmosphereRadius", atmosphereRadius);
            
            if (_settingsUpToDate) return;
            //Debug.LogError("Updating atmosphere properties");
            
            material.SetInt("has_atmosphere", atmosphereSettings.hasAtmosphere ? 1 : 0);
            material.SetInt ("numInScatteringPoints", atmosphereSettings.inScatteringPoints);
            material.SetInt ("numOpticalDepthPoints", atmosphereSettings.opticalDepthPoints);
            material.SetFloat ("densityFalloff", atmosphereSettings.densityFalloff);

            // Strength of (rayleigh) scattering is inversely proportional to wavelength^4
            PRNG random = new PRNG (atmosphereSettings.shadingSeed);
            Vector3 wv = atmosphereSettings.wavelengths;
            if (atmosphereSettings.randomizeShading)
            {
                if (atmosphereSettings.realisticColors)
                {
                    var n = random.Range(0, 3);
                    var d = random.Range(-100f, 100f);
                    Vector3 baseWv = atmosphereSettings.wavelengths;
                    wv = n switch
                    {
                        // blue sky
                        0 => new Vector3(baseWv.x + d, baseWv.y + d / 5, baseWv.z + d / 10),
                        // red sky
                        1 => new Vector3(baseWv.z + d / 10, baseWv.y + d / 5, baseWv.x + d),
                        // green sky
                        2 => new Vector3(baseWv.x + d, baseWv.z + d / 10, baseWv.y + d / 5),
                        _ => wv
                    };
                }
                else
                {
                    wv += new Vector3(random.Range(-500, 500), random.Range(-50, 50), random.Range(-10, 10));
                }
            }
            float scatterX = Mathf.Pow (400 / wv.x, 4);
            float scatterY = Mathf.Pow (400 / wv.y, 4);
            float scatterZ = Mathf.Pow (400 / wv.z, 4);
            material.SetVector ("scatteringCoefficients", new Vector3 (scatterX, scatterY, scatterZ) * atmosphereSettings.scatteringStrength);
            material.SetFloat ("intensity", atmosphereSettings.intensity);

            PrecomputeOutScattering ();
            material.SetTexture ("_BakedOpticalDepth", _opticalDepthTexture);
            
            _settingsUpToDate = true;
        }

        private void PrecomputeOutScattering () {
            if (_opticalDepthTexture == null || !_opticalDepthTexture.IsCreated ()) {
                ComputeHelper.CreateRenderTexture (ref _opticalDepthTexture, atmosphereSettings.textureSize);
                opticalDepthCompute.SetTexture (0, "Result", _opticalDepthTexture);
                opticalDepthCompute.SetInt ("textureSize", atmosphereSettings.textureSize);
                opticalDepthCompute.SetInt ("numOutScatteringSteps", atmosphereSettings.opticalDepthPoints);
                opticalDepthCompute.SetFloat ("atmosphereRadius", (1 + atmosphereSettings.atmosphereScale));
                opticalDepthCompute.SetFloat ("densityFalloff", atmosphereSettings.densityFalloff);
                ComputeHelper.Run (opticalDepthCompute, atmosphereSettings.textureSize, atmosphereSettings.textureSize);
            }
        }


        [Serializable]
        public class AtmosphereSettings
        {
            public bool randomizeShading;
            public int shadingSeed = 0;

            public bool realisticColors = true;
            
            public bool hasAtmosphere;
            
            public int textureSize = 256;

            public int inScatteringPoints = 10;
            public int opticalDepthPoints = 100;
            public float densityFalloff = 4.3f;

            public Vector3 wavelengths = new Vector3 (700, 530, 460);

            public float scatteringStrength = 21.23f;
            public float intensity = 1;

            [Range (0, 10)]
            public float atmosphereScale = 0.322f;
            
            public void RandomizeShading(bool rand)
            {
                randomizeShading = rand;
                shadingSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
        }
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            _settingsUpToDate = false;
        }
        public AtmosphereSettings GetSettings()
        {
            return atmosphereSettings;
        }

        public void SetSettings(AtmosphereSettings s)
        {
            atmosphereSettings = s;
            _settingsUpToDate = false;
        }
    }
}