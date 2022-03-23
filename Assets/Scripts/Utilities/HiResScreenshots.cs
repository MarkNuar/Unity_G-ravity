using System;
using System.IO;
using UnityEngine;

namespace Utilities
{
    public class HiResScreenshots : MonoBehaviour {
        public int resWidth = 2550; 
        public int resHeight = 3300;
 
        private bool _takeHiResShot = false;

        private string _storePath; 
        
        private string ScreenShotName(int width, int height) {
            return string.Format("{0}screen_{1}x{2}_{3:yyyy-MM-dd_HH-mm-ss}.png", 
                _storePath, 
                width, height, DateTime.Now);
        }

        private void Start()
        {
            _storePath = Application.persistentDataPath + Path.DirectorySeparatorChar +
                         "screenshots" + Path.DirectorySeparatorChar;
            if (!Directory.Exists(_storePath))
            {
                Directory.CreateDirectory(_storePath);
            }
        }

        // take screenshot from code
        public void TakeHiResShot() {
            _takeHiResShot = true;
        }
 
        void LateUpdate() {
            _takeHiResShot |= Input.GetKeyDown(KeyCode.F11);
            if (_takeHiResShot) {
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                GameManager.Instance.GetMainCamera().targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                GameManager.Instance.GetMainCamera().Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                GameManager.Instance.GetMainCamera().targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(resWidth, resHeight);
                System.IO.File.WriteAllBytes(filename, bytes);
                Debug.Log(string.Format("Took screenshot to: {0}", filename));
                _takeHiResShot = false;
            }
        }
    }
}