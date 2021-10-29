using System;
using CBodies;
using CBodies.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing
{
    public class CBodyPreview : MonoBehaviour
    {
        public Canvas canvas;
        public TextMeshProUGUI bodyName;
        public Button selectButton;
        
        [SerializeField] private GameObject selectionMesh;
        [SerializeField] private GameObject cBodyGameObject;
        
        public CBody cBody;

        private void Awake()
        {
            cBody = cBodyGameObject.AddComponent<CBody>();
        }

        public void SelectCBody()
        {
            Color color = Color.cyan;
            color.a = 0.2f;
            selectionMesh.GetComponent<MeshRenderer>().material.color = color;
            selectionMesh.SetActive(true);
            // Make it not clickable
            canvas.sortingOrder = 0;
        }

        public void HideSelectionMesh()
        {
            selectionMesh.SetActive(false);
        }

        public void DeselectCBody()
        {
            // Make it clickable
            canvas.sortingOrder = 1;
            selectionMesh.SetActive(false);
        }
        
    }
}
