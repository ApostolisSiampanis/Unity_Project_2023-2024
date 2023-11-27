#if VISTA
using UnityEngine;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace Pinwheel.Vista
{
    public class CoroutineHandle
    {
#if UNITY_EDITOR
        public EditorCoroutine coroutine { get; set; }
#else
        public Coroutine coroutine { get; set; }
#endif
        internal CoroutineManager manager { get; set; }
    }
}
#endif
