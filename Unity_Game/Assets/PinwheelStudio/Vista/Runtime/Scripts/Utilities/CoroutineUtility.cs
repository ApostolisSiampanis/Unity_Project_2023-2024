#if VISTA
using System.Collections;
#if UNITY_EDITOR
using Unity.EditorCoroutines.Editor;
#endif

namespace Pinwheel.Vista
{
    public class CoroutineUtility
    {
        private static CoroutineManager manager { get; set; }

        public static CoroutineHandle StartCoroutine(IEnumerator coroutine)
        {
            if (manager == null)
            {
                manager = CoroutineManager.CreateInstance();
            }

            CoroutineHandle handler = new CoroutineHandle();
            handler.manager = manager;
#if UNITY_EDITOR
            handler.coroutine = EditorCoroutineUtility.StartCoroutine(coroutine, manager);
#else
            handler.coroutine = manager.StartCoroutine(coroutine);
#endif
            return handler;
        }

        public static void StopCoroutine(CoroutineHandle coroutine)
        {
            if (coroutine.coroutine != null)
            {
#if UNITY_EDITOR
                EditorCoroutineUtility.StopCoroutine(coroutine.coroutine);
#else
                coroutine.manager.StopCoroutine(coroutine.coroutine);
#endif
            }
        }
    }
}
#endif
