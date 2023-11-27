#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;
using UnityEngine.Networking;
using UnityEditor;
using System;

namespace Pinwheel.VistaEditor
{
    public class UpdateChecker
    {
        [System.Serializable]
        private class VersionResponse
        {
            public int major;
            public int minor;
            public int patch;
        }

        private static readonly string PREF_PREFIX = "vista-check-update-";

        internal static bool CheckedToday()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            return EditorPrefs.HasKey(PREF_PREFIX + dateString);
        }

        internal static void CheckForUpdate()
        {
            string dateString = DateTime.Now.ToString("yyyy-MM-dd");
            EditorPrefs.SetBool(PREF_PREFIX + dateString, true);
            CoroutineUtility.StartCoroutine(ICheckForUpdate());
        }

        private static IEnumerator ICheckForUpdate()
        {
            string url = "https://api.pinwheelstud.io/vista/version-info";
            UnityWebRequest r = UnityWebRequest.Get(url);
            yield return r.SendWebRequest();
            if (r.result == UnityWebRequest.Result.Success)
            {
                VersionResponse response = new VersionResponse();
                EditorJsonUtility.FromJsonOverwrite(r.downloadHandler.text, response);

                if (response.major > VersionInfo.major ||
                    response.minor > VersionInfo.minor ||
                    response.patch > VersionInfo.patch)
                {
                    Debug.Log($"VISTA: New version {response.major}.{response.minor}.{response.patch} is available, please update using the Package Manager.");
                }
            }
        }
    }
}
#endif
