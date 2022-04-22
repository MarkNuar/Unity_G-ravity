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
        
        public CBodyDrag positionDrag;

        public TMP_Text cBodyName;
        public TMP_Text cBodyPosition;
        public Image cBodyPositionBGD;
        
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
            
            positionDrag.dragHandle.enabled = true;
            cBodyPosition.enabled = true;
            cBodyPositionBGD.enabled = true;
            
            // Make it not clickable
            buttonCanvas.sortingOrder = 0;
        }

        public void ToggleSelectionHUD(bool show)
        {
            positionDrag.dragHandle.enabled = show;
            cBodyName.enabled = show;
            cBodyPosition.enabled = show;
            cBodyPositionBGD.enabled = show;
        }

        public void DeselectCBody()
        {
            Selected = false;
            
            positionDrag.dragHandle.enabled = false;
            cBodyName.enabled = true;
            cBodyPosition.enabled = false;
            cBodyPositionBGD.enabled = false;
            
            // Make it clickable
            buttonCanvas.sortingOrder = 1;
        }

        public void ShowEditingHUD(bool showHUD)
        {
            selectButton.interactable = showHUD;
            cBodyName.enabled = showHUD;
            
            if(Selected)
                ToggleSelectionHUD(showHUD);
        }
    }
}
