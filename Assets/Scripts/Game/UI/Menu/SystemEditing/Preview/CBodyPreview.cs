using System;
using CBodies;
using CBodies.Settings;
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
        public TMP_Text cBodyPosition;
        
        public LineRenderer lineRenderer;
        
        public CBody cBody;

        public bool Selected { get; set; }

        // private void Start()
        // {
        //     buttonCanvas.worldCamera = GameManager.Instance.GetMainCamera();
        // }

        public void SelectCBody()
        {
            Selected = true;
            if (cBody.cBodyGenerator.cBodySettings.cBodyType != CBodySettings.CBodyType.Star)
            {
                velocityArrow.arrowHead.enabled = false;
                velocityArrow.arrowBody.enabled = false;
                positionDrag.dragHandle.enabled = true;
                
                cBodyPosition.enabled = true;
            }
            // Make it not clickable
            buttonCanvas.sortingOrder = 0;
        }

        public void HideSelectionHUD()
        {
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;
            positionDrag.dragHandle.enabled = false;
            
            cBodyName.enabled = false;
            
            cBodyPosition.enabled = false;
        }

        public void DeselectCBody()
        {
            Selected = false;
            
            velocityArrow.arrowHead.enabled = false;
            velocityArrow.arrowBody.enabled = false;
            positionDrag.dragHandle.enabled = false;
            
            cBodyName.enabled = true;
            
            cBodyPosition.enabled = false;
            
            // Make it clickable
            buttonCanvas.sortingOrder = 1;
        }

        public void ToggleHUD(bool showHUD)
        {
            selectButton.interactable = showHUD;
            cBodyName.enabled = showHUD;
        }
    }
}
