using System;
using UnityEngine;

namespace UI.Menu.SystemEditing
{
    public class ClampPanel : MonoBehaviour
    {
        public RectTransform panelTransform;
        
        private void Update()
        {
            if (Camera.main is null) return;
            Debug.Log(transform.position);
            Vector3 position = Camera.main.WorldToScreenPoint(this.transform.position);
            Debug.Log(position);
            panelTransform.position = position;
        }
    }
}
