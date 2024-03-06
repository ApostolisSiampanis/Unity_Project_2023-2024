#if VISTA
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Vista
{
    public static class SpawnUtilities
    {
        public static readonly string ROOT_NAME = "~VistaSpawnerRoot";

        public static GameObject Spawn(GameObject original)
        {
            GameObject g = null;
#if UNITY_EDITOR
            bool isPrefab = PrefabUtility.IsPartOfPrefabAsset(original);

            if (isPrefab)
            {
                g = PrefabUtility.InstantiatePrefab(original) as GameObject;
            }
            else
            {
                g = GameObject.Instantiate<GameObject>(original);
            }

            string undoName = string.Format("Spawn {0}", original.name);
            Undo.RegisterCreatedObjectUndo(g, undoName);
#else
            g = GameObject.Instantiate<GameObject>(original);
#endif
            return g;
        }
    }
}
#endif
