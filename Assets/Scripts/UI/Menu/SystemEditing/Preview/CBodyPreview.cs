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
        
        public CBodyArrow velocityArrow;
        public CBodyDrag positionDrag;
        
        [SerializeField] private GameObject selectionMesh;

        public CBody cBody;

        public void SelectCBody()
        {
            velocityArrow.arrowHead.enabled = true;
            velocityArrow.arrowBody.enabled = true;

            positionDrag.dragHandle.enabled = true; 

            // Make it not clickable
            buttonCanvas.sortingOrder = 0;
        }

        public void HideSelectionHUD()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;

            positionDrag.dragHandle.enabled = false;
        }

        public void DeselectCBody()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;

            positionDrag.dragHandle.enabled = false;

            // Make it clickable
            buttonCanvas.sortingOrder = 1;
        }
    }
}
