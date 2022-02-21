using CBodies;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Menu.SystemEditing.Preview
{
    public class CBodyPreview :  MonoBehaviour//, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Canvas buttonCanvas;
        public Button selectButton;
        
        public CBodyArrow velocityArrow;
        public CBodyDrag positionDrag;

        public TMP_Text cBodyName;
        
        public LineRenderer lineRenderer;
        
        public CBody cBody;

        public bool Selected { get; set; }

        public void SelectCBody()
        {
            Selected = true;
            
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
            cBodyName.enabled = false;

            positionDrag.dragHandle.enabled = false;
        }

        public void DeselectCBody()
        {
            Selected = false;
            
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;
            cBodyName.enabled = true;

            positionDrag.dragHandle.enabled = false;

            // Make it clickable
            buttonCanvas.sortingOrder = 1;
        }
    }
}
