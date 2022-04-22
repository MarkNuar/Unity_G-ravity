using System;
using CBodies;
using CBodies.Settings;
using Game;
using Game.UI.Menu;
using Game.UI.Menu.SystemEditing;
using Game.UI.Menu.SystemEditing.Preview;
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

        public SystemEditingMenu systemEditingMenu;
        
        private Vector3 _storedPos;
        private Quaternion _storedRot;
        private bool _isPlaying;

        public bool trackCBody;
        public Transform transformToTrack;
        public int trackedCBodyIndex;
        
        // Start is called before the first frame update
        private void Start()
        {
            cam = GameManager.Instance.GetMainCamera();
            _isPlaying = false;
        }
        

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F9))
            {
                if (!_isPlaying)
                {
                    systemEditingMenu.CloseAllMenu();
                    
                    CBodyPreview[] previews = FindObjectsOfType<CBodyPreview>();
                    foreach (var preview in previews)
                    {
                        preview.ShowEditingHUD(false);   
                    }

                    var camTransform = cam.transform;
                    _storedPos = camTransform.position;
                    
                    if (trackCBody)
                    {
                        camTransform.parent = transformToTrack;
                        var targetPosition = transformToTrack.position;
                        camTransform.position = new Vector3(targetPosition.x, targetPosition.y,
                            camTransform.position.z);
                    }
                    else
                    {
                        camTransform.position = new Vector3(0, 0, camTransform.position.z);
                    }

                    cam.transform.rotation = Quaternion.Euler(0,0,0);
                    
                    //_storedRot = transform1.rotation;
                    cam.orthographic = false;
                    flyCamera.enabled = true;
                    editMenu.SetActive(false);
                    //pauseMenu.SetActive(false);
                    _isPlaying = true;
                    // SunShadowCaster ssc = FindObjectOfType<SunShadowCaster>();
                    // ssc.trackCamera = true;
                }
                else
                {
                    CBodyPreview[] previews = FindObjectsOfType<CBodyPreview>();
                    foreach (var preview in previews)
                    {
                        preview.ShowEditingHUD(true);
                        preview.cBody.transform.rotation = Quaternion.Euler(0,0,0);
                    }

                    cam.transform.parent = null;
                
                    var camTransform = cam.transform;
                    camTransform.position = _storedPos;
                    camTransform.rotation = Quaternion.Euler(0,0,0);
                    
                    cam.orthographic = true;
                    flyCamera.enabled = false;
                    editMenu.SetActive(true);
                    //pauseMenu.SetActive(true);
                    _isPlaying = false;
                    // SunShadowCaster ssc = FindObjectOfType<SunShadowCaster>();
                    // ssc.trackCamera = false;
                }
            }
        }
    }
}