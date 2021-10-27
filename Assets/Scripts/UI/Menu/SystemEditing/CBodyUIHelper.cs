using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.Menu.SystemEditing
{
    public class CBodyUIHelper : MonoBehaviour
    {
        //public Image image;
        public GameObject CBodyUIElement;
        public TextMeshProUGUI bodyName;
        public Button selectButton;
        [SerializeField] private GameObject selectionMesh;

        // public void HighLightCBody()
        // {
        //     selectionMesh.GetComponent<MeshRenderer>().material.color = Color.white;
        //     selectionMesh.SetActive(true);
        // }
        //
        // public void StopHighLight()
        // {
        //     selectionMesh.SetActive(false);
        // }

        public void SelectCBody()
        {
            Color color = Color.cyan;
            color.a = 0.2f;
            selectionMesh.GetComponent<MeshRenderer>().material.color = color;
            selectionMesh.SetActive(true);
        }

        // public void StopSelect()
        // {
        //     selectionMesh.SetActive(false);
        // }

        public void HideSelectionMesh()
        {
            selectionMesh.SetActive(false);
        }
    }
}
