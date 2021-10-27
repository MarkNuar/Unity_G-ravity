using System;
using UnityEngine;

namespace UI.Menu.SystemEditing
{
    public class CameraController : MonoBehaviour
    {
        public Camera cam;
        
        public float maxScrollSpeed = 25;
        public float panSensitivity = 0.3f;
        public float minZoom = 1.5f;
        public float maxZoom = 60;

        public float basePower = 1.1f;

        private bool _enableControl = true;
        private bool _restoringControl = false;
        private float _controlledCameraZoomSpeed = -1;
        private Vector3 _cinematicTarget;
        private Vector3 _initialCameraPosition;
        private float _initialCameraZoom;
        
        private Vector3 _dragOrigin;

        private void Start()
        {
            _initialCameraPosition = cam.transform.position;
            _initialCameraZoom = cam.orthographicSize;
        }

        private void Update()
        {
            // CAMERA CONTROLLED
            if (_enableControl) 
            {
                if (Input.GetMouseButtonDown(0))
                {
                    _dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                }
    
                if (Input.GetMouseButton(0))
                {
                    Vector3 difference = _dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
                    MoveCamera(difference);
                }
                
                var scrollDelta = Input.mouseScrollDelta.y;
                if (scrollDelta == 0) return;
                {
                    ZoomCamera(scrollDelta);
                }
            }
            // CAMERA NOT CONTROLLED
            else switch (_restoringControl)
            {
                // ZOOMING INTO A PLANET
                case false:
                {
                    Vector3 posDifference =  _cinematicTarget - cam.transform.position;
                    var zoomDifference = cam.orthographicSize - minZoom;
                    CinematicZoom(posDifference/5, zoomDifference/2);
                    break;
                }
                // LEAVING A PLANET
                case true:
                {
                    Vector3 posDifference =  _initialCameraPosition - cam.transform.position;
                    var zoomDifference = cam.orthographicSize - _initialCameraZoom;
                    if (posDifference.magnitude < 1 && Mathf.Abs(zoomDifference) < 1)
                    {
                        Debug.Log("giving back control");
                        _enableControl = true;
                        _restoringControl = false;
                    }
                    else
                    {
                        CinematicZoom(posDifference/5, zoomDifference/2);
                    }
                    break;
                }
                   
            }
            // Debug.Log("ENABLING " +_enableControl + ", RESTORING " + _restoringControl);
        }

        private void MoveCamera(Vector3 difference)
        {
            Vector3 position = cam.transform.position;
            position = Vector3.Lerp(position, position + difference,
                panSensitivity * Time.deltaTime);
            cam.transform.position = position;
        }

        private void ZoomCamera(float scrollDelta)
        {
            var orthographicSize = cam.orthographicSize;
            orthographicSize -= scrollDelta;
            cam.orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
            orthographicSize = cam.orthographicSize;
            Vector3 difference = (cam.ScreenToWorldPoint(Input.mousePosition) - cam.transform.position);
            
            var zoomExpFactor = Mathf.Clamp(
                (Mathf.Pow(basePower, 
                    Mathf.Min((orthographicSize - minZoom)*10,  difference.magnitude)) - 1), 0, maxScrollSpeed);
            
            difference.Normalize();
            difference *= Mathf.Sign(scrollDelta);

            MoveCamera(difference * zoomExpFactor);
        }
        
        private void CinematicZoom(Vector3 posDifference, float zoomDifference)
        {
            Vector3 position = cam.transform.position;
            position = Vector3.Lerp(position, position + posDifference,
                panSensitivity * Time.deltaTime);
            cam.transform.position = position;
            
            var orthographicSize = cam.orthographicSize;
            // orthographicSize -= difference.magnitude * _controlledCameraZoomSpeed;
            var increment = zoomDifference * _controlledCameraZoomSpeed;
            orthographicSize = Mathf.Lerp(orthographicSize, orthographicSize - increment,
                panSensitivity * Time.deltaTime);
            cam.orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
        }

        public void LookAt(Vector3 pos)
        {
            _enableControl = false;
            _restoringControl = false; 
            Transform camTransform = cam.transform;
            if (!_restoringControl)
            {
                _initialCameraPosition = camTransform.position;
                _initialCameraZoom = cam.orthographicSize;
            }
            _cinematicTarget = new Vector3(pos.x, pos.y, camTransform.position.z);
            var initialCameraDistance = (_cinematicTarget - camTransform.position).magnitude;
            _controlledCameraZoomSpeed = 0.5f * (_initialCameraZoom - minZoom) / (initialCameraDistance - 0);
        }

        public void FreeCam()
        {
            // _enableControl = true;
            _restoringControl = true;
        }
        
    }
}
