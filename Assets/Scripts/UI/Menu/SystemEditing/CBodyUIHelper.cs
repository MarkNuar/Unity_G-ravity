using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing
{
    public class CBodyUIHelper : MonoBehaviour
    {
        public GameObject cBodyUIElement;
        public Canvas canvas;
        public TextMeshProUGUI bodyName;
        public Button selectButton;
        [SerializeField] private GameObject selectionMesh;

        public void SelectCBody()
        {
            Color color = Color.cyan;
            color.a = 0.2f;
            selectionMesh.GetComponent<MeshRenderer>().material.color = color;
            selectionMesh.SetActive(true);
            canvas.sortingOrder = 0;
        }

        public void HideSelectionMesh()
        {
            //canvas.sortingOrder = 1;
            selectionMesh.SetActive(false);
        }

        public void DeselectCBody()
        {
            canvas.sortingOrder = 1;
            selectionMesh.SetActive(false);
        }

        // public void HideName()
        // {
        //     _hiddenName = true;
        //     _storedName = bodyName.text;
        //     bodyName.text = "";
        // }
        //
        // public void ShowName()
        // {
        //     if (!_hiddenName) return;
        //     _hiddenName = false;
        //     bodyName.text = _storedName;
        // }
    }
}
