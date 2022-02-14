using UnityEngine;

namespace Utilities
{
    public class FPSCounter : MonoBehaviour
    {
        float _deltaTime = 0.0f;

        private void Update()
        {
            _deltaTime += (Time.unscaledDeltaTime - _deltaTime) * 0.1f;
        }

        private void OnGUI()
        {
            int w = Screen.width, h = Screen.height;
 
            GUIStyle style = new GUIStyle();
 
            Rect rect = new Rect(0, 0, w, h * 2 / 100);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = h * 2 / 100;
            style.normal.textColor = Color.gray;
            float fps = 1.0f / _deltaTime;
            string text = $"{fps:0.} fps";
            GUI.Label(rect, text, style);
        }
    }
}