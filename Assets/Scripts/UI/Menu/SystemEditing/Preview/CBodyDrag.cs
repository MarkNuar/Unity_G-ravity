using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing.Preview
{
    public class CBodyDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private Camera _cam;
        private CameraController _cameraController;
        private Transform _dragHandleTransform;

        public Image dragHandle;
        public float handleOffset = 10f;
        
        public GameObject objDragged;
        public Arrow velocityArrow;
        public UnityEvent<Vector3> onDrag;
        
        private void Awake()
        {
            _cam = GameManager.Instance.GetMainCamera();
            _cameraController = _cam.GetComponent<CameraController>();
            _cameraController.onCameraDrag.AddListener(UpdateHandlePosition);
            _cameraController.onCameraZoom.AddListener(UpdateHandlePosition);
            
            _dragHandleTransform = dragHandle.transform;
            // Vector3 pos = _cam.WorldToScreenPoint(objDragged.transform.position);
            // pos.x -= handleOffset;
            // _dragHandleTransform.position = pos;

            //UpdateHandlePosition();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("Begin Dragging");
        }

        public void OnDrag(PointerEventData data)
        {
            if(data.button != 0) return;
            
            UpdateHandlePosition(data.position, true);

            UpdateDraggedObjPosition();
            
            velocityArrow.UpdateArrowOrigin();
            onDrag?.Invoke(objDragged.transform.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("End Dragging");
        }

        private void UpdateDraggedObjPosition()
        {
            Vector3 draggedTargetPos = _dragHandleTransform.position;
            draggedTargetPos.x += handleOffset;
            draggedTargetPos = _cam.ScreenToWorldPoint(draggedTargetPos);
            draggedTargetPos.z = objDragged.transform.position.z;
            objDragged.transform.position = draggedTargetPos;
        }
        
        public void UpdateHandlePosition()
        {
            Debug.Log(objDragged.transform.position);
            UpdateHandlePosition(objDragged.transform.position, false);
        }

        private void UpdateHandlePosition(Vector3 targetPosition, bool isFromInput)
        {
            Vector3 handleTargetPos;
            if (isFromInput)
            {
                handleTargetPos = targetPosition;
            }
            else
            {
                handleTargetPos = _cam.WorldToScreenPoint(targetPosition);
                handleTargetPos.x -= handleOffset;
            }
            Vector3 handlePos = _dragHandleTransform.position;
            if (isFromInput)
            {
                handleTargetPos.y = handlePos.y;
            }
            handleTargetPos.z = handlePos.z;
            handlePos = handleTargetPos;
            _dragHandleTransform.position = handlePos;
        }
    }
}
