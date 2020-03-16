using UnityEngine;

namespace UIFW
{
    public class CanvasScript : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(this);
        }
    }
}
