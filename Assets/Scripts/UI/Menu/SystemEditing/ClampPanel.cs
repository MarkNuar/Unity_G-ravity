using UnityEngine;

namespace UI.Menu.SystemEditing
{
    public class ClampPanel : MonoBehaviour
    {
        public RectTransform panelTransform;
        
        private void Update()
        {
            if (Camera.main is null) return;
            Vector3 position1 = this.transform.position;
            Debug.Log(position1);
            Vector3 position = Camera.main.WorldToScreenPoint(position1);
            Debug.Log(position);
            panelTransform.position = position;
        }
    }
}
