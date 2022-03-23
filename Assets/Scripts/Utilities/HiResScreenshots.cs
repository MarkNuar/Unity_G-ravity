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
            
            if (!_takeHiResShot) return;
            ScreenCapture.CaptureScreenshot(ScreenShotName(resWidth, resHeight));
            Debug.Log("Captured!");
            _takeHiResShot = false;
        }
    }
}