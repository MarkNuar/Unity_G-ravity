using UnityEngine;

namespace UI.Menu.SystemEditing
{
    public class CBodyElement : MonoBehaviour
    {
        public void DeleteElement()
        {
            Destroy(this);
        }
    }
}
