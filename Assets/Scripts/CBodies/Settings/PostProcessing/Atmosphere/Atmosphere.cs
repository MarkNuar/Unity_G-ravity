using System;
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
        
        public Texture2D blueNoise;
        
        private RenderTexture _opticalDepthTexture;

        public bool hasPhysicChanged;

        public void SetAtmosphereProperties(Material material, float cBodyRadius)
        {
            // if (_settingsUpToDate) return;
            material.SetInt("has_atmosphere", atmosphereSettings.hasAtmosphere ? 1 : 0);
            
            var atmosphereRadius = (1 + atmosphereSettings.atmosphereScale) * cBodyRadius;
            
            material.SetInt ("numInScatteringPoints", atmosphereSettings.inScatteringPoints);
            material.SetInt ("numOpticalDepthPoints", atmosphereSettings.opticalDepthPoints);
            material.SetFloat ("atmosphereRadius", atmosphereRadius);
            material.SetFloat ("planetRadius", cBodyRadius);
            material.SetFloat ("densityFalloff", atmosphereSettings.densityFalloff);

            // Strength of (rayleigh) scattering is inversely proportional to wavelength^4
            PRNG random = new PRNG (atmosphereSettings.colorSeed);
            Vector3 wv = atmosphereSettings.wavelengths;
            if (atmosphereSettings.randomizeColor)
            {
                wv += new Vector3(random.Range(-500, 500), random.Range(-100, 100), random.Range(-50, 50));
            }
            float scatterX = Mathf.Pow (400 / wv.x, 4);
            float scatterY = Mathf.Pow (400 / wv.y, 4);
            float scatterZ = Mathf.Pow (400 / wv.z, 4);
            material.SetVector ("scatteringCoefficients", new Vector3 (scatterX, scatterY, scatterZ) * atmosphereSettings.scatteringStrength);
            material.SetFloat ("intensity", atmosphereSettings.intensity);
            material.SetFloat ("ditherStrength", atmosphereSettings.ditherStrength);
            material.SetFloat ("ditherScale", atmosphereSettings.ditherScale);
            material.SetTexture ("_BlueNoise", blueNoise);

            // todo check if it has sense
            if (hasPhysicChanged)
            {
                PrecomputeOutScattering ();
                material.SetTexture ("_BakedOpticalDepth", _opticalDepthTexture);
            }
            
            // _settingsUpToDate = true;
        }
        
        void PrecomputeOutScattering () {
            if (hasPhysicChanged || _opticalDepthTexture == null || !_opticalDepthTexture.IsCreated ()) {
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
            public bool randomizeColor;
            public int colorSeed = 0;

            public bool hasAtmosphere;
            
            public int textureSize = 256;

            public int inScatteringPoints = 10;
            public int opticalDepthPoints = 100;
            public float densityFalloff = 4.3f;

            public Vector3 wavelengths = new Vector3 (700, 530, 460);
            
            public float scatteringStrength = 21.23f;
            public float intensity = 1;

            public float ditherStrength = 1f;
            public float ditherScale = 3.89f;
            
            [Range (0, 10)]
            public float atmosphereScale = 0.322f;
            
            public void UpdateColorSeed(bool rand)
            {
                colorSeed = rand ? _prng.Next(-10000, 10000) : 0;
            }
        }
        
        // MEMENTO PATTERN
        public void InitSettings()
        {
            
        }
        public AtmosphereSettings GetSettings()
        {
            return atmosphereSettings;
        }

        public void SetSettings(AtmosphereSettings s)
        {
            atmosphereSettings = s;
            // _settingsUpToDate = false;
        }
    }
}