using UnityEngine;

namespace StarView
{
    public class StarView : MonoBehaviour {

        public MeshRenderer starPrefab;
        public Vector2 radiusMinMax;
        public int count = 1000;
        const float CalibrationDst = 2000;
        public Vector2 brightnessMinMax;

        private Camera _cam;

        void Start ()
        {
            _cam = GameManager.Instance.GetMainCamera();
            //var sw = System.Diagnostics.Stopwatch.StartNew ();
            if (_cam) {
                float starDst = _cam.farClipPlane - radiusMinMax.y;
                float scale = starDst / CalibrationDst;

                for (int i = 0; i < count; i++) {
                    MeshRenderer star = Instantiate (starPrefab, Random.onUnitSphere * starDst, Quaternion.identity, transform);
                    float t = SmallestRandomValue (6);
                    star.transform.localScale = Vector3.one * Mathf.Lerp (radiusMinMax.x, radiusMinMax.y, t) * scale;
                    star.material.color = Color.Lerp (Color.black, star.material.color, Mathf.Lerp (brightnessMinMax.x, brightnessMinMax.y, t));
                }
            }
            //Debug.Log (sw.ElapsedMilliseconds);
        }

        float SmallestRandomValue (int iterations) {
            float r = 1;
            for (int i = 0; i < iterations; i++) {
                r = Mathf.Min (r, Random.value);
            }
            return r;
        }

        void LateUpdate () {
            if (_cam != null) {
                transform.position = _cam.transform.position;
            }
        }
    }
}