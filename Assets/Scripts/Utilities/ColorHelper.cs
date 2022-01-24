using UnityEngine;

namespace Utilities
{
    public static class ColorHelper {

        // Create random colour within a given range of saturation and value
        public static Color Random (PRNG random, float satMin, float satMax, float valMin, float valMax) {
            return Color.HSVToRGB (random.Value (), random.Range (satMin, satMax), random.Range (valMin, valMax));
        }

        public static Color TweakHSV (Color colRGB, float deltaH, float deltaS, float deltaV) {
            float hue, sat, val;
            Color.RGBToHSV (colRGB, out hue, out sat, out val);
            return Color.HSVToRGB ((hue + deltaH) % 1, sat + deltaS, val + deltaV);
        }

        public static Color RandomSimilar (PRNG random, Color original, float maxHueDelta, float maxSatDelta, float maxValDelta) {
            float hue, sat, val;
            Color.RGBToHSV (original, out hue, out sat, out val);
            hue = (hue + random.SignedValue () * maxHueDelta) % 1;
            sat += random.SignedValue () * maxSatDelta;
            val += random.SignedValue () * maxValDelta;
            return Color.HSVToRGB (hue, sat, val);
        }

        public static Color RandomSimilar (PRNG random, Color original) {
            float hue, sat, val;
            Color.RGBToHSV (original, out hue, out sat, out val);
            hue = (hue + random.SignedValue () * 0.25f) % 1;
            sat += random.Sign () * random.Range (0.2f, 0.4f);
            sat = Mathf.Clamp (sat, 0.2f, 0.8f);
            val += random.Sign () * random.Range (0.2f, 0.4f);
            val = Mathf.Clamp (val, 0.2f, 0.8f);
            return Color.HSVToRGB (hue, sat, val);
        }

        public static Color RandomContrasting (PRNG random, Color original) {
            float hue, sat, val;
            Color.RGBToHSV (original, out hue, out sat, out val);
            hue = (hue + 0.5f + random.SignedValue () * 0.1f) % 1;
            sat += random.SignedValue () * 0.2f;
            val = (val < 0.5f) ? random.Range (val + 0.2f, 0.9f) : random.Range (0.1f, val - 0.2f);
            return Color.HSVToRGB (hue, sat, val);
        }
        
        public static float Map (float x, float x1, float x2, float y1,  float y2)
        {
            var m = (y2 - y1) / (x2 - x1);
            var c = y1 - m * x1; // point of interest: c is also equal to y2 - m * x2, though float math might lead to slightly different results.
 
            return m * x + c;
        }

        public static Color Vector3ToColor(Vector3 v)
        {
            return new Color(v.x, v.y, v.z);
        }
        
        // cosine based palette, 4 vec3 params
        // Inigo Quilez Palette colors
        public static Color Palette(float t, Vector3 a, Vector3 b, Vector3 c, Vector3 d )
        {
            Vector3 twoPiCtd = 6.28318f * (c * t + d);
            Vector3 r = new Vector3(
                Mathf.Cos(twoPiCtd.x),
                Mathf.Cos(twoPiCtd.y),
                Mathf.Cos(twoPiCtd.z)
            );
            Vector3 p = new Vector3(
                b.x * r.x,
                b.y * r.y,
                b.z * r.z
            );
            return Vector3ToColor(a + p);
        }
    }
}