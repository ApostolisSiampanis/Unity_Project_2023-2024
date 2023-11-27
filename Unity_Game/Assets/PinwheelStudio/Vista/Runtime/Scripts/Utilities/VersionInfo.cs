#if VISTA
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Pinwheel.Vista
{
    public static class VersionInfo
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod]
#endif
        public static void OnInitialize()
        {
            VersionManager.collectVersionInfoCallback += OnCollectVersionInfo;
        }

        private static void OnCollectVersionInfo(Collector<string> versionStrings)
        {
            versionStrings.Add($"{productName} {versionLabel}");
        }

        public static int major
        {
            get
            {
                return 2023;
            }
        }

        public static int minor
        {
            get
            {
                return 1;
            }
        }

        public static int patch
        {
            get
            {
                return 7;
            }
        }

        public static bool isBeta
        {
            get
            {
                return false;
            }
        }

        public static string versionLabel
        {
            get
            {
                return $"{major}.{minor}.{patch}{(isBeta ? "b" : "")}";
            }
        }

        public static string productName
        {
            get
            {
                return "Vista";
            }
        }
    }
}
#endif
