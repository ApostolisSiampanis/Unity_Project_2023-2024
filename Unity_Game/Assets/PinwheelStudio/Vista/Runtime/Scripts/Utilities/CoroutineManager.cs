#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    [AddComponentMenu("")]
    internal class CoroutineManager : MonoBehaviour
    {
        public static CoroutineManager CreateInstance()
        {
            GameObject g = new GameObject("Vista Coroutine Manager");
            g.hideFlags = HideFlags.HideAndDontSave;
            CoroutineManager managerComponent = g.AddComponent<CoroutineManager>();
            return managerComponent;
        }
    }
}
#endif
