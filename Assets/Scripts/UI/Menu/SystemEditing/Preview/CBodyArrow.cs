using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing.Preview
{
    public class CBodyArrow :  MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Image arrowHead;
        public Image arrowBody;
        //public LineRenderer arrowBody; 
        
        public GameObject followTarget;
        
        private Camera _cam;
        private CameraController _cameraController;

        private Vector2 _diff;
        
        public UnityEvent<Vector3, float> onDrag;

        public float minArrowLen = 10f;
        [HideInInspector] public float maxArrowLen;
        // 0 -> use minArrowY
        // 1 -> use maxArrowY

        private void Awake()
        {
            maxArrowLen = Mathf.Min(Screen.width, Screen.height) * 0.4f;
            
            _cam = GameManager.Instance.GetMainCamera();
            _cameraController = _cam.GetComponent<CameraController>();
            
            _cameraController.onCameraDrag.AddListener(UpdateArrowOrigin);
            //_cameraController.onCameraZoom.AddListener(UpdateDiff);
            _cameraController.onCameraZoom.AddListener(UpdateArrowOrigin);
            
            UpdateDiff();
        }

        public void UpdateArrowOrigin()
        {
            UpdateArrow(false, Vector2.one);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {}

        public void OnDrag(PointerEventData data)
        {
            if (data.button != 0) return;

            Vector2 targetPos = _cam.WorldToScreenPoint(followTarget.transform.position);
            UpdateArrow(true, data.position - targetPos);
            
            var magnitude = _diff.magnitude;
            var percent = (magnitude - minArrowLen) / (maxArrowLen - minArrowLen);
            onDrag?.Invoke(_diff.normalized,percent);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            UpdateDiff();
        }
        
        // Percent tells the percentage of the vector lenght between min and max velocity
        public void SetArrowHeadPosition(Vector3 direction, float percent)
        {
            var len = (maxArrowLen - minArrowLen) * percent + minArrowLen;
            UpdateArrow(true, direction * len);
        }

        private void UpdateArrow(bool setNewDiff, Vector2 newDiff)
        {
            Vector2 targetPos = _cam.WorldToScreenPoint(followTarget.transform.position);
            if (setNewDiff)
            {
                _diff = CorrectDiff(newDiff);
            }
            Vector3 res = targetPos + _diff;
            res.z = 0;
            arrowHead.rectTransform.position = res;
            arrowBody.rectTransform.sizeDelta = new Vector2(arrowBody.rectTransform.sizeDelta.x , (_diff).magnitude);
            arrowBody.rectTransform.position = targetPos;
        }

        private Vector2 CorrectDiff(Vector2 diff)
        {
            if (diff.magnitude < minArrowLen)
                diff = diff.normalized * minArrowLen;
            else if (diff.magnitude > maxArrowLen)
                diff = diff.normalized * maxArrowLen;

            diff.x = 0;
            if (diff.y < minArrowLen)
                diff.y = minArrowLen;

            //Vector3.Project(diff, Vector3.up);
            
            return diff;
        }
        
        private void UpdateDiff()
        {
            _diff = CorrectDiff(arrowHead.rectTransform.position - _cam.WorldToScreenPoint(followTarget.transform.position));
        }
    }
}
