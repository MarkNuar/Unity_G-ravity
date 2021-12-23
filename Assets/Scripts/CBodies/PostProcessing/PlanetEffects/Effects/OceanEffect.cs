using Game;
using UnityEngine;

namespace CBodies.PostProcessing.PlanetEffects.Effects
{
    public class OceanEffect {

        Light light;
        protected Material material;

        public void UpdateSettings (CBodyGenerator generator, Shader shader) {
            if (material == null || material.shader != shader) {
                material = new Material (shader);
            }

            if (light == null) {
                light = GameObject.FindObjectOfType<SunShadowCaster> ()?.GetComponent<Light> ();
            }

            Vector3 centre = generator.transform.position;
            float radius = generator.GetOceanRadius ();
            material.SetVector ("oceanCentre", centre);
            material.SetFloat ("oceanRadius", radius);

            material.SetFloat ("planetScale", generator.BodyScale);
            if (light) {
                material.SetVector ("dirToSun", -light.transform.forward);
            } else {
                material.SetVector ("dirToSun", Vector3.up);
                Debug.Log ("No SunShadowCaster found");
            }
            generator.cBodySettings.ocean.SetOceanProperties (material);
        }

        public Material GetMaterial () {
            return material;
        }

    }
}