using System;
using CBodies;
using CBodies.Settings;
using Game.UI.Menu;
using Game.UI.Menu.SystemEditing;
using UnityEngine;
using Physics = CBodies.Settings.Physics.Physics;

namespace Utilities
{
    public class FakePlayMode : MonoBehaviour
    {
        public Camera cam;
        public FlyCamera flyCamera;
        public GameObject editMenu;
        public GameObject pauseMenu;
        public GameObject starView;
        
        private Vector3 _storedPos;
        private Quaternion _storedRot;
        private bool _isPlaying;

        // Start is called before the first frame update
        private void Start()
        {
            cam = GameManager.Instance.GetMainCamera();
            _isPlaying = false;
        }


        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.P)) return;
            if (!_isPlaying)
            {
                var transform1 = cam.transform;
                _storedPos = transform1.position;
                _storedRot = transform1.rotation;
                cam.orthographic = false;
                flyCamera.enabled = true;
                editMenu.SetActive(false);
                //pauseMenu.SetActive(false);
                starView.SetActive(true);
                _isPlaying = true;
            }
            else
            {
                var transform1 = cam.transform;
                transform1.position = _storedPos;
                transform1.rotation = _storedRot;
                cam.orthographic = true;
                flyCamera.enabled = false;
                editMenu.SetActive(true);
                //pauseMenu.SetActive(true);
                starView.SetActive(false);
                _isPlaying = false;
            }
        }
    }
}