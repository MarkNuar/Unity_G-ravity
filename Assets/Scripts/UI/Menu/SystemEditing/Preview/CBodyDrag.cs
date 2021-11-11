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
        //public float handleOffset = 10f;
        
        public GameObject objToDrag;
        public CBodyArrow velocityArrow;
        public UnityEvent<Vector3> onDrag;


        private float _diff;
        
        private void Awake()
        {
            dragHandle.alphaHitTestMinimumThreshold = 1f;
            
            _cam = GameManager.Instance.GetMainCamera();
            _cameraController = _cam.GetComponent<CameraController>();
            _cameraController.onCameraDrag.AddListener(UpdateDragHandlePosition);
            _cameraController.onCameraZoom.AddListener(UpdateDragHandlePosition);
            
            _dragHandleTransform = dragHandle.transform;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _diff = _dragHandleTransform.position.x - eventData.position.x;
            //Debug.Log("Begin Dragging");
        }

        public void OnDrag(PointerEventData data)
        {
            if(data.button != 0) return;
            
            UpdateDragHandlePosition(data.position, true);

            UpdateObjToDragPos();
            
            velocityArrow.UpdateArrowOrigin();
            onDrag?.Invoke(objToDrag.transform.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("End Dragging");
        }

        private void UpdateObjToDragPos()
        {
            Vector3 draggedTargetPos = _dragHandleTransform.position;
            //draggedTargetPos.x += handleOffset;
            draggedTargetPos = _cam.ScreenToWorldPoint(draggedTargetPos);
            draggedTargetPos.z = objToDrag.transform.position.z;
            objToDrag.transform.position = draggedTargetPos;
        }
        
        private void UpdateDragHandlePosition()
        {
            UpdateDragHandlePosition(objToDrag.transform.position, false);
        }

        private void UpdateDragHandlePosition(Vector3 targetPosition, bool isFromInput)
        {
            //Debug.Log("Target world pos: " + targetPosition);
            Vector3 handleTargetPos;
            if (isFromInput)
            {
                handleTargetPos = targetPosition;
                handleTargetPos.x += _diff;
            }
            else
            {
                handleTargetPos = _cam.WorldToScreenPoint(targetPosition);
                //handleTargetPos.x -= handleOffset;
            }
            //Debug.Log("Handle target screen pos not fixed: "+ handleTargetPos);
            Vector3 handlePos = _dragHandleTransform.position;
            //Debug.Log("Handle initial screen pos: " + handlePos);
            if (isFromInput)
            {
                handleTargetPos.y = handlePos.y;
            }
            handleTargetPos.z = handlePos.z;
            handlePos = handleTargetPos;
            //Debug.Log("Handle target screen pos fixed: "+ handlePos);
            _dragHandleTransform.position = handlePos;
        }

        public void ResetDragHandlePosition()
        {
            Vector3 targetPosition = _cam.WorldToScreenPoint(objToDrag.transform.position);
            //targetPosition.x -= handleOffset;
            _dragHandleTransform.position = targetPosition;
        }
    }
}
