using System;
using CBodies;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing.Preview
{
    public class CBodyPreview :  MonoBehaviour//, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Canvas buttonCanvas;
        public Button selectButton;
        
        public Arrow velocityArrow;
        public CBodyDrag positionDrag;
        
        [SerializeField] private GameObject selectionMesh;
        [SerializeField] private GameObject cBodyGameObject;
        
        public CBody cBody;
        
        private void Awake()
        {
            cBody = cBodyGameObject.AddComponent<CBody>();
        }
        
        public void SelectCBody()
        {
            velocityArrow.arrowHead.enabled = true;
            velocityArrow.arrowBody.enabled = true;

            positionDrag.dragHandle.enabled = true;
            //positionDrag.UpdateHandlePosition();

            // Color color = Color.cyan;
            // color.a = 0.2f;
            // selectionMesh.GetComponent<MeshRenderer>().material.color = color;
            //
            // selectionMesh.SetActive(true);

            // Make it not clickable
            buttonCanvas.sortingOrder = 0;
        }

        public void HideSelectionMesh()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;

            positionDrag.dragHandle.enabled = false;

            // selectionMesh.SetActive(false);
        }

        public void DeselectCBody()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;

            positionDrag.dragHandle.enabled = false;

            // Make it clickable
            buttonCanvas.sortingOrder = 1;
            
            // selectionMesh.SetActive(false);
        }
        
    }
}
