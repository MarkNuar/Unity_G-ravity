using System;
using CBodies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing.Preview
{
    public class CBodyPreview :  MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Canvas buttonCanvas;
        public Button selectButton;
        
        public Arrow velocityArrow;
       
        public UnityEvent<Vector3> onDrag;
        
        [SerializeField] private GameObject selectionMesh;
        [SerializeField] private GameObject cBodyGameObject;
        
        public CBody cBody;

        private Camera _cam;
        private Transform _transform;

        private void Awake()
        {
            cBody = cBodyGameObject.AddComponent<CBody>();
        }

        private void Start()
        {
            _cam = GameManager.Instance.GetMainCamera();
            _transform = transform;
        }

        public void SelectCBody()
        {
            velocityArrow.arrowHead.enabled = true;
            velocityArrow.arrowBody.enabled = true;
                
            Color color = Color.cyan;
            color.a = 0.2f;
            selectionMesh.GetComponent<MeshRenderer>().material.color = color;
            selectionMesh.SetActive(true);
            // Make it not clickable
            //buttonCanvas.sortingOrder = 0;
        }

        public void HideSelectionMesh()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;
            
            selectionMesh.SetActive(false);
        }

        public void DeselectCBody()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;
            
            // Make it clickable
            //buttonCanvas.sortingOrder = 1;
            selectionMesh.SetActive(false);
        }
        
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            //Debug.Log("Begin Dragging");
        }

        public void OnDrag(PointerEventData data)
        {
            if (data.button != 0) return;
            // Debug.Log("Dragging");
            Vector3 pos = _transform.position;
            Vector2 target = data.position;
            Vector3 worldTarget = _cam.ScreenToWorldPoint(target);
            worldTarget.z = pos.z;
            worldTarget.y = pos.y;
            _transform.position = worldTarget;
            
            velocityArrow.UpdateArrowOrigin();

            onDrag?.Invoke(transform.position);
        }
        
        public void OnEndDrag(PointerEventData eventData)
        {
            //Debug.Log("End Dragging");
        }
    }
}
