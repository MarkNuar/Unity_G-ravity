using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.UI.Menu.SystemEditing.Preview
{
    public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler
    {
        public GameObject dragTarget;
        
        public void OnBeginDrag(PointerEventData eventData)
        {
            
        }

        public void OnDrag(PointerEventData eventData)
        {
            if(eventData.button != 0) return;
            
            Debug.LogError("Dragging");
            dragTarget.transform.position = GameManager.Instance.GetMainCamera().ScreenToWorldPoint(eventData.position);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            
        }
    }
}