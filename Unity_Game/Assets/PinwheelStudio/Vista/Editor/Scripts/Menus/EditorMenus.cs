#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Unity.Collections.LowLevel.Unsafe;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Unity.Collections;

namespace Pinwheel.VistaEditor
{
    public static class EditorMenus
    {
        //[MenuItem("Window/Vista/Internal/Toggle Leak Detection Mode")]
        //public static void ToggleLeakDetectionMode()
        //{
        //    NativeLeakDetectionMode mode = UnsafeUtility.GetLeakDetectionMode();
        //    if (mode == NativeLeakDetectionMode.Disabled)
        //    {
        //        UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.EnabledWithStackTrace);
        //        Debug.Log("Current mode: " + NativeLeakDetectionMode.EnabledWithStackTrace);
        //    }
        //    else
        //    {
        //        UnsafeUtility.SetLeakDetectionMode(NativeLeakDetectionMode.Disabled);
        //        Debug.Log("Current mode: " + NativeLeakDetectionMode.Disabled);
        //    }

        //}

        [MenuItem("GameObject/3D Object/Vista/Vista Manager", priority = -10000)]
        public static void CreateVistaManagerAndDefaultBiome(MenuCommand cmd)
        {
            VistaManager manager = VistaManager.CreateInstanceInScene();
            if (cmd.context != null && cmd.context is GameObject root)
            {
                manager.transform.parent = root.transform;
            }

            LocalProceduralBiome biome = LocalProceduralBiome.CreateInstanceInScene(manager);
            Selection.activeObject = manager;
            EditorSceneManager.MarkSceneDirty(manager.gameObject.scene);
        }

        [MenuItem("GameObject/3D Object/Vista/Local Procedural Biome")]
        public static void CreateLocalProceduralBiome(MenuCommand cmd)
        {
            VistaManager manager = null;
            if (cmd.context != null && cmd.context is GameObject root)
            {
                manager = root.GetComponentInParent<VistaManager>();
            }
            LocalProceduralBiome biome = LocalProceduralBiome.CreateInstanceInScene(manager);
            Selection.activeObject = biome;
            EditorSceneManager.MarkSceneDirty(biome.gameObject.scene);
        }

        //[MenuItem("GameObject/3D Object/Vista/Local Procedural Biome", true)]
        //public static bool ValidateCreateLocalProceduralBiome()
        //{
        //    VistaManager manager = null;
        //    if (Selection.activeGameObject != null)
        //    {
        //        manager = Selection.activeGameObject.GetComponentInParent<VistaManager>();
        //    }
        //    return manager != null;
        //}        

        [MenuItem("Window/Vista/Contact/Send An Email")]
        public static void SendEmail()
        {
            EditorCommon.OpenEmailEditor(
                Links.SUPPORT_EMAIL,
                "[Vista] SHORT_QUESTION_HERE",
                "YOUR_QUESTION_IN_DETAIL");
        }

        [MenuItem("Window/Vista/Contact/Discord")]
        public static void OpenDiscord()
        {
            Application.OpenURL(Links.DISCORD);
        }

        [MenuItem("Window/Vista/Learning Resources/Documentation")]
        public static void OpenDocumentation()
        {
            Application.OpenURL(Links.DOC);
        }

        [MenuItem("Window/Vista/Learning Resources/Youtube")]
        public static void OpenYoutubeChannel()
        {
            Application.OpenURL(Links.YOUTUBE);
        }

        [MenuItem("Window/Vista/Learning Resources/Facebook")]
        public static void OpenFacebook()
        {
            Application.OpenURL(Links.FACEBOOK);
        }

        [MenuItem("Window/Vista/Version Info")]
        public static void ShowVersionInfo()
        {
            List<string> versionStrings = VersionManager.GetVersionStrings();
            StringBuilder sb = new StringBuilder();
            foreach (string s in versionStrings)
            {
                sb.AppendLine(s);
            }

            EditorUtility.DisplayDialog("Version Info", sb.ToString(), "OK");
        }

        [MenuItem("Window/Vista/Leave A Review")]
        public static void ShowStorePageForReview()
        {
            Application.OpenURL($"{Links.STORE_PAGE}#reviews");
        }
    }
}
#endif
