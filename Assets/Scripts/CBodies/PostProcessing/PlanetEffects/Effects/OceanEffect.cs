using Game;
using UnityEngine;

namespace CBodies.PostProcessing.PlanetEffects.Effects
{
    public class OceanEffect {
        
        private Light _light;
        private Material _material;

        public void UpdateSettings (CBodyGenerator generator, Shader shader) {
            if (_material == null || _material.shader != shader) {
                _material = new Material (shader);
            }

            if (_light == null) {
                _light = Object.FindObjectOfType<Light> ();
            }

            Vector3 centre = generator.transform.position;
            float radius = generator.GetOceanRadius ();
            _material.SetVector ("oceanCentre", centre);
            _material.SetFloat ("oceanRadius", radius);

            _material.SetFloat ("planetScale", generator.BodyScale);
            if (_light) {
                _material.SetVector ("dirToSun", -_light.transform.forward);
            } else {
                _material.SetVector ("dirToSun", Vector3.up);
                Debug.Log ("No SunShadowCaster found");
            }
            generator.cBodySettings.ocean.SetOceanProperties (_material);
        }

        public Material GetMaterial () {
            return _material;
        }

    }
}