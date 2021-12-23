using UnityEngine;
using UnityEngine.Events;

namespace Game.UI.Menu.SystemEditing
{
    public class CameraController : MonoBehaviour
    {
        public Camera cam;
        
        public float panSensitivity = 0.3f;

        public float minZoom = 1.5f;
        public float maxZoom = 60;
        public float zoomSpeed = 5f;

        private bool _enableControl = true;
        private bool _restoringControl = false;
        private float _cinematicMinZoo = 0;
        private Vector3 _cinematicTarget;
        private Vector3 _initialCameraPosition;
        private float _initialCameraZoom;
        public float cinematicPrecision = 0.1f;
        
        private Vector3 _dragOrigin;

        public bool isDragging = false;

        public UnityEvent onCameraZoom; 
        public UnityEvent onCameraDrag; 

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
                if (posDifference.magnitude < cinematicPrecision && Mathf.Abs(zoomDifference) < cinematicPrecision 
                    || Input.GetMouseButton(1) || Input.mouseScrollDelta.y != 0) // stop the zoom out if the user presses a button
                {
                    _restoringControl = false;
                }
                else
                {
                    CinematicZoom(posDifference/5, zoomDifference/2);
                }
            }

            // CAMERA CONTROLLED
            if (_enableControl ) 
            {
                // do not move the camera is the mouse is over the editing menu
                //if (!IsPointerOverUIElement() && !Input.GetMouseButton(0))
                {
                    if (Input.GetMouseButtonDown(1))
                    {
                        _dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                    }
                
                    if (Input.GetMouseButton(1))
                    {
                        Vector3 difference = _dragOrigin - cam.ScreenToWorldPoint(Input.mousePosition);
                        if (difference.magnitude > 10*Mathf.Epsilon)
                        {
                            isDragging = true;
                        }
                        MoveCamera(difference);
                    }

                    if (Input.GetMouseButtonUp(1) && isDragging)
                    {
                        isDragging = false;
                    }

                    var scrollDelta = Input.mouseScrollDelta.y;
                    if (scrollDelta == 0) return;
                    {
                        ZoomCamera(scrollDelta);
                    }
                }
            }
            // CAMERA NOT CONTROLLED, zooming into a planet
            else
            {
                Vector3 posDifference =  _cinematicTarget - cam.transform.position;
                var zoomDifference = cam.orthographicSize - _cinematicMinZoo;
                if (posDifference.magnitude > cinematicPrecision || Mathf.Abs(zoomDifference) > cinematicPrecision)
                {
                    CinematicZoom(posDifference/5, zoomDifference/4);
                }
            }
        }

        private void MoveCamera(Vector3 difference)
        {
            if (!CanCameraMove()) return;
            Vector3 position = cam.transform.position;
            position = Vector3.Lerp(position, position + difference,
                panSensitivity * Time.deltaTime);
            cam.transform.position = position;
            
            onCameraDrag?.Invoke();
        }

        private void ZoomCamera(float scrollDelta)
        {
            var orthographicSize = cam.orthographicSize;
            
            var amount = scrollDelta > 0 ? zoomSpeed : -zoomSpeed;

            if (Mathf.Abs(orthographicSize - minZoom) > Mathf.Epsilon
                && Mathf.Abs(maxZoom - orthographicSize) > Mathf.Epsilon)
            {
                float multiplier = (1.0f / orthographicSize * amount);
                cam.transform.position += (cam.ScreenToWorldPoint(Input.mousePosition) - transform.position) * multiplier; 
            }
            orthographicSize -= amount;
            
            cam.orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
            
            onCameraZoom?.Invoke();
        }
        
        private void CinematicZoom(Vector3 posDifference, float zoomDifference)
        {
            Vector3 position = cam.transform.position;
            position = Vector3.Lerp(position, position + posDifference,
                panSensitivity * Time.deltaTime);
            cam.transform.position = position;
            
            var orthographicSize = cam.orthographicSize;
            orthographicSize = Mathf.Lerp(orthographicSize, orthographicSize - zoomDifference,
                panSensitivity * Time.deltaTime);
            cam.orthographicSize = Mathf.Clamp(orthographicSize, minZoom, maxZoom);
        }

        // todo
        private bool CanCameraMove()
        {
            // todo check if at least one planet is in camera view
            return true;
        }
        
        public void LockCamAt(Vector3 pos, float cBodyRadius, bool fromCreation)
        {
            _cinematicMinZoo = cBodyRadius + 3;
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
            if (fromCreation)
            {
                _initialCameraPosition = _cinematicTarget; // when zooming out, remain on the created planet
            }
        }

        public void FreeCam()
        {
            if (_enableControl != false) return;
            _enableControl = true;
            _restoringControl = true;
        }

    }
}
