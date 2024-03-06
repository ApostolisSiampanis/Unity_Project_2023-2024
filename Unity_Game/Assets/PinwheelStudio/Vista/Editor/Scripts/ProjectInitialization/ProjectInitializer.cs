using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Pinwheel.VistaEditor
{
    [InitializeOnLoad]
    public static class ProjectInitializer
    {
        public const string KW_VISTA = "VISTA";

        public static bool isSearcherInstalled { get; set; }
        public static bool isEditorCoroutinesInstalled { get; set; }

        [InitializeOnLoadMethod] 
        public static void Initialize()
        {
            ResetPackageState();
            SetupDependencyPackages();
            SetupScriptingSymbols();
        }

        private static void ResetPackageState()
        {
            isSearcherInstalled = false;
            isEditorCoroutinesInstalled = false;
        }

        private static void SetupDependencyPackages()
        {
            List<Type> loadedTypes = GetAllLoadedTypes();
            foreach (Type t in loadedTypes)
            {
                if (!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("UnityEditor.Searcher"))
                {
                    isSearcherInstalled = true;
                }
                if (!string.IsNullOrEmpty(t.Namespace) && t.Namespace.StartsWith("Unity.EditorCoroutines"))
                {  
                    isEditorCoroutinesInstalled = true;
                }
            }

            if (!isSearcherInstalled)
            {
                Debug.Log("VISTA: Installing dependency package [com.unity.searcher]");
                Client.Add("com.unity.searcher");
            }
            if (!isEditorCoroutinesInstalled)
            {
                Debug.Log("VISTA: Installing dependency package [com.unity.editorcoroutines]");
                Client.Add("com.unity.editorcoroutines");
            }
        }

        private static void SetupScriptingSymbols()
        {
            BuildTarget buildTarget = EditorUserBuildSettings.activeBuildTarget;
            BuildTargetGroup buildTargetGroup = BuildPipeline.GetBuildTargetGroup(buildTarget);

            string scriptingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            string newScriptingSymbols = scriptingSymbols;

            if (isSearcherInstalled)
            {
                newScriptingSymbols = AddKeywordToScriptingSymbols(KW_VISTA, newScriptingSymbols);
            }
            else
            {
                newScriptingSymbols = RemoveKeywordFromScriptingSymbols(KW_VISTA, newScriptingSymbols);
            }

            if (!string.IsNullOrEmpty(newScriptingSymbols) && !newScriptingSymbols.Equals(scriptingSymbols))
            {
                PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, newScriptingSymbols);
            }
        }

        private static List<string> SplitScriptingSymbolsToList(string scriptingSymbols)
        {
            string[] splitSymbols = scriptingSymbols.Split(new string[] { ";" }, System.StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < splitSymbols.Length; ++i)
            {
                string s = splitSymbols[i].Replace(" ", "");
                splitSymbols[i] = s;
            }
            return new List<string>(splitSymbols);
        }

        private static string CombineSymbolsListToString(List<string> symbols)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < symbols.Count; ++i)
            {
                sb.Append(symbols[i]).Append(";");
            }
            return sb.ToString();
        }

        public static string AddKeywordToScriptingSymbols(string kw, string scriptingSymbols)
        {
            List<string> splitSymbols = SplitScriptingSymbolsToList(scriptingSymbols);
            if (!splitSymbols.Contains(kw))
            {
                splitSymbols.Add(kw);
            }
            return CombineSymbolsListToString(splitSymbols);
        }

        public static string RemoveKeywordFromScriptingSymbols(string kw, string scriptingSymbols)
        {
            List<string> splitSymbols = SplitScriptingSymbolsToList(scriptingSymbols);
            splitSymbols.RemoveAll(s => s.Equals(kw));
            return CombineSymbolsListToString(splitSymbols);
        }

        private static List<Type> GetAllLoadedTypes()
        {
            List<Type> loadedTypes = new List<Type>();
            List<string> typeName = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var t in assembly.GetTypes())
                {
                    if (t.IsVisible && !t.IsGenericType)
                    {
                        typeName.Add(t.Name);
                        loadedTypes.Add(t);
                    }
                }
            }
            return loadedTypes;
        }
    }
}
