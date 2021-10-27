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
            // BRINGS BACK CAMERA TO ORIGINAL POSITION
            if (_restoringControl)
            {
                Vector3 posDifference =  _initialCameraPosition - cam.transform.position;
                var zoomDifference = cam.orthographicSize - _initialCameraZoom;
                if (posDifference.magnitude < 1 && Mathf.Abs(zoomDifference) < 1 || Input.GetMouseButton(0) || Input.mouseScrollDelta.y != 0)
                {
                    Debug.Log("giving back control");
                    _restoringControl = false;
                }
                else
                {
                    CinematicZoom(posDifference/5, zoomDifference/2);
                }
            }
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
            // CAMERA NOT CONTROLLED, zooming into a planet
            else
            {
                Vector3 posDifference =  _cinematicTarget - cam.transform.position;
                var targetZoom = minZoom; // todo: target zoom should depend on selected planet size! 
                var zoomDifference = cam.orthographicSize - targetZoom;
                CinematicZoom(posDifference/5, zoomDifference/2);
            }
        }

        private void MoveCamera(Vector3 difference)
        {
            if (!CanCameraMove()) return;
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

        private bool CanCameraMove()
        {
            // todo check if at least one planet is in camera view
            return true;
        }
        
        public void LockCamAt(Vector3 pos)
        {
            _enableControl = false;
            _restoringControl = false; 
            Transform camTransform = cam.transform;
            if (!_restoringControl)
            {
                _initialCameraPosition = camTransform.position;
                _initialCameraZoom = cam.orthographicSize;
            }

            Vector3 position = camTransform.position;
            _cinematicTarget = new Vector3(pos.x, pos.y, position.z);
            var initialCameraDistance = (_cinematicTarget - position).magnitude;
            _controlledCameraZoomSpeed = 0.5f * (_initialCameraZoom - minZoom) / (initialCameraDistance - 0);
        }

        public void FreeCam()
        {
            _enableControl = true;
            _restoringControl = true;
        }
        
    }
}
