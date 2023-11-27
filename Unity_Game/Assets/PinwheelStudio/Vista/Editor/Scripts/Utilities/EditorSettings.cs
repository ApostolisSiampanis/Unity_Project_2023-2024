#if VISTA
using UnityEngine;
using Pinwheel.Vista.Graph;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System;
using UnityEngine.Networking;
using Pinwheel.Vista;

namespace Pinwheel.VistaEditor
{
    //[CreateAssetMenu(menuName = "Vista/Internal/Editor Settings")]
    [ExecuteInEditMode]
    public class EditorSettings : ScriptableObject
    {
        [System.Serializable]
        public class GraphEditorSettings
        {
            public enum TextureDisplayMode
            {
                Height,
                Mask
            }

            public const int MIN_VIS_QUALITY = 1;
            public const int MAX_VIS_QUALITY = 10;

            [SerializeField]
            private int m_terrainVisualizationQuality;
            public int terrainVisualizationQuality
            {
                get
                {
                    return m_terrainVisualizationQuality;
                }
                set
                {
                    m_terrainVisualizationQuality = Mathf.Clamp(value, MIN_VIS_QUALITY, MAX_VIS_QUALITY);
                }
            }

            [SerializeField]
            private bool m_showWaterLevel;
            public bool showWaterLevel
            {
                get
                {
                    return m_showWaterLevel;
                }
                set
                {
                    m_showWaterLevel = value;
                }
            }

            [SerializeField]
            private float m_waterLevel;
            public float waterLevel
            {
                get
                {
                    return m_waterLevel;
                }
                set
                {
                    m_waterLevel = value;
                }
            }

            [SerializeField]
            private bool m_showGridline;
            public bool showGrid
            {
                get
                {
                    return m_showGridline;
                }
                set
                {
                    m_showGridline = value;
                }
            }

            [SerializeField]
            private TextureDisplayMode m_defaultTextureDisplayMode;
            public TextureDisplayMode defaultTextureDisplayMode
            {
                get
                {
                    return m_defaultTextureDisplayMode;
                }
                set
                {
                    m_defaultTextureDisplayMode = value;
                }
            }

            public enum ViewportGradientOptions
            {
                BlackRed, WhiteRed, BlackWhite, BlackRedYellowGreen, Rainbow, Custom
            }

            [SerializeField]
            private ViewportGradientOptions m_viewportGradient;
            public ViewportGradientOptions viewportGradient
            {
                get
                {
                    return m_viewportGradient;
                }
                set
                {
                    m_viewportGradient = value;
                }
            }

            [SerializeField]
            private Texture2D m_blackRedGradient;
            [SerializeField]
            private Texture2D m_whiteRedGradient;
            [SerializeField]
            private Texture2D m_blackWhiteGradient;
            [SerializeField]
            private Texture2D m_blackRedYellowGreenGradient;
            [SerializeField]
            private Texture2D m_rainbowGradient;

            [SerializeField]
            private Texture2D m_customViewportGradient;
            public Texture2D customViewportGradient
            {
                get
                {
                    return m_customViewportGradient;
                }
                set
                {
                    m_customViewportGradient = value;
                }
            }

            public Texture2D GetViewportGradient()
            {
                if (viewportGradient == ViewportGradientOptions.BlackRed)
                    return m_blackRedGradient;
                else if (viewportGradient == ViewportGradientOptions.WhiteRed)
                    return m_whiteRedGradient;
                else if (viewportGradient == ViewportGradientOptions.BlackWhite)
                    return m_blackWhiteGradient;
                else if (viewportGradient == ViewportGradientOptions.BlackRedYellowGreen)
                    return m_blackRedYellowGreenGradient;
                else if (viewportGradient == ViewportGradientOptions.Rainbow)
                    return m_rainbowGradient;
                else if (viewportGradient == ViewportGradientOptions.Custom)
                    return m_customViewportGradient;
                return null;
            }



            public GraphEditorSettings()
            {
                m_terrainVisualizationQuality = 5;
                m_showWaterLevel = false;
                m_waterLevel = 0;
                m_showGridline = true;
                m_defaultTextureDisplayMode = TextureDisplayMode.Height;
            }
        }

        [System.Serializable]
        public class GeneralSettings
        {
            [SerializeField]
            private bool m_enableAffLinks;
            public bool enableAffLinks
            {
                get
                {
                    return m_enableAffLinks;
                }
                set
                {
                    m_enableAffLinks = value;
                }
            }

            public GeneralSettings()
            {
                m_enableAffLinks = true;
            }
        }

        [System.Serializable]
        public class TroubleshootingSettings
        {
            [SerializeField]
            private bool m_dontExecuteGraphOnSelection;
            public bool dontExecuteGraphOnSelection
            {
                get
                {
                    return m_dontExecuteGraphOnSelection;
                }
                set
                {
                    m_dontExecuteGraphOnSelection = value;
                }
            }

            [SerializeField]
            private bool m_enableTroubleshootingMode;
            public bool enableTroubleshootingMode
            {
                get
                {
                    return m_enableTroubleshootingMode;
                }
                set
                {
                    m_enableTroubleshootingMode = value;
                }
            }

            private const string GRAPH_EXEC_LOG_FILE_NAME = "GraphExecLog";

            public TroubleshootingSettings()
            {
                m_enableTroubleshootingMode = false;
                m_dontExecuteGraphOnSelection = false;
            }

            internal static string GetExecLogFilePath(TerrainGraph graph)
            {
                string tmpFolder = GetTempFolderPath();
                string logFilePath = Path.Combine(tmpFolder, $".{GRAPH_EXEC_LOG_FILE_NAME}_{graph.name}.txt");
                return logFilePath;
            }

            internal static string[] GetGraphExecLog(TerrainGraph graph)
            {
                string logFilePath = GetExecLogFilePath(graph);
                if (File.Exists(logFilePath))
                {
                    return File.ReadAllLines(logFilePath);
                }
                else
                {
                    return new string[0];
                }
            }

            public static bool IsTroubleshootingModeEnabled()
            {
                EditorSettings editorSettings = EditorSettings.Get();
                return editorSettings.troubleshootingSettings.enableTroubleshootingMode;
            }
        }

        [System.Serializable]
        public class MarketingSettings
        {
            [System.Serializable]
            public class NewsEntry
            {
                public string title;
                public string description;
                public string link;
            }

            [System.Serializable]
            public class ListNewsResponse
            {
                public NewsEntry[] entries;
            }

            private NewsEntry[] m_news;

            [System.Serializable]
            public class AssetEntry : IDisposable
            {
                public string imageUrl;
                public string name;
                public string description;
                public string link;
                public string assemblyName;
                public string promotionText;

                public Texture2D texture { get; private set; }
                public bool isInstalled { get; private set; }

                public void Init()
                {
                    Dispose();
                    CoroutineUtility.StartCoroutine(IDownloadImage());
                    isInstalled = string.IsNullOrEmpty(assemblyName) ? false : EditorCommon.HasAssembly(assemblyName);
                }

                private IEnumerator IDownloadImage()
                {
                    if (string.IsNullOrEmpty(imageUrl))
                        yield break;
                    UnityWebRequest r = UnityWebRequestTexture.GetTexture(imageUrl);
                    yield return r.SendWebRequest();
                    if (r.result == UnityWebRequest.Result.Success)
                    {
                        texture = (r.downloadHandler as DownloadHandlerTexture).texture;
                    }
                    r.Dispose();
                }

                public void Dispose()
                {
                    if (texture != null)
                    {
                        UnityEngine.Object.DestroyImmediate(texture);
                    }
                }
            }

            [System.Serializable]
            public class ListModulesResponse
            {
                public AssetEntry[] entries;
            }

            private AssetEntry[] m_vistaModules;
            private AssetEntry[] m_featuredAssets;

            public void Init()
            {
                CoroutineUtility.StartCoroutine(IListNews());
                CoroutineUtility.StartCoroutine(IListVistaModules());
                CoroutineUtility.StartCoroutine(IListFeaturedAssets());
            }

            private IEnumerator IListNews()  
            {
                string url = "https://api.pinwheelstud.io/news";
                UnityWebRequest r = UnityWebRequest.Get(url);
                yield return r.SendWebRequest();
                ListNewsResponse response = new ListNewsResponse();
                EditorJsonUtility.FromJsonOverwrite(r.downloadHandler.text, response);
                m_news = response.entries;
            }

            private IEnumerator IListVistaModules()
            {
                string url = "https://api.pinwheelstud.io/vista/modules";
                UnityWebRequest r = UnityWebRequest.Get(url);
                yield return r.SendWebRequest();
                ListModulesResponse response = new ListModulesResponse();
                EditorJsonUtility.FromJsonOverwrite(r.downloadHandler.text, response);
                m_vistaModules = response.entries;
                foreach (AssetEntry e in m_vistaModules)
                {
                    e.Init();
                }
            }

            private IEnumerator IListFeaturedAssets()
            {
                string url = "https://api.pinwheelstud.io/vista/other-products";
                UnityWebRequest r = UnityWebRequest.Get(url);
                yield return r.SendWebRequest();
                ListModulesResponse response = new ListModulesResponse();
                EditorJsonUtility.FromJsonOverwrite(r.downloadHandler.text, response);
                m_featuredAssets = response.entries;
                foreach (AssetEntry e in m_featuredAssets)
                {
                    e.Init();
                }
            }

            public void CleanUp()
            {
                if (m_vistaModules != null)
                {
                    foreach (AssetEntry e in m_vistaModules)
                    {
                        e.Dispose();
                    }
                    m_vistaModules = null;
                }

                if (m_featuredAssets != null)
                {
                    foreach (AssetEntry e in m_featuredAssets)
                    {
                        e.Dispose();
                    }
                    m_featuredAssets = null;
                }
            }

            internal AssetEntry[] GetVistaModules()
            {
                if (m_vistaModules != null)
                {
                    return m_vistaModules;
                }
                else
                {
                    return new AssetEntry[0];
                }
            }

            internal AssetEntry[] GetFeaturedAssets()
            {
                if (m_featuredAssets != null)
                {
                    return m_featuredAssets;
                }
                else
                {
                    return new AssetEntry[0];
                }
            }

            internal NewsEntry[] GetNews()
            {
                if (m_news != null)
                {
                    return m_news;
                }
                else
                {
                    return new NewsEntry[0];
                }
            }
        }

        private static EditorSettings s_instance;

        [SerializeField]
        private GeneralSettings m_generalSettings;
        public GeneralSettings generalSettings
        {
            get
            {
                return m_generalSettings;
            }
            set
            {
                m_generalSettings = value;
            }
        }

        [SerializeField]
        private GraphEditorSettings m_graphEditorSettings;
        public GraphEditorSettings graphEditorSettings
        {
            get
            {
                return m_graphEditorSettings;
            }
            set
            {
                m_graphEditorSettings = value;
            }
        }

        [SerializeField]
        private TroubleshootingSettings m_troubleshootingSettings;
        public TroubleshootingSettings troubleshootingSettings
        {
            get
            {
                return m_troubleshootingSettings;
            }
            set
            {
                m_troubleshootingSettings = value;
            }
        }

        [SerializeField]
        private MarketingSettings m_marketingSettings;
        public MarketingSettings marketingSettings
        {
            get
            {
                return m_marketingSettings;
            }
            set
            {
                m_marketingSettings = value;
            }
        }

        public void Reset()
        {
            m_generalSettings = new GeneralSettings();
            m_graphEditorSettings = new GraphEditorSettings();
            m_troubleshootingSettings = new TroubleshootingSettings();
            m_marketingSettings = new MarketingSettings();
        }

        public static EditorSettings Get()
        {
            if (s_instance == null)
            {
                s_instance = Resources.Load<EditorSettings>("Vista/EditorSettings");
            }
            if (s_instance == null)
            {
                s_instance = ScriptableObject.CreateInstance<EditorSettings>();
                Debug.LogWarning("VISTA: Editor Settings asset does not exist. Please re-import the package.");
            }
            return s_instance;
        }

        private void OnEnable()
        {
            m_marketingSettings.Init();
            if (!UpdateChecker.CheckedToday())
            {
                UpdateChecker.CheckForUpdate();
            }
        }

        private void OnDisable()
        {
            m_marketingSettings.CleanUp();
        }

        public static string GetTempFolderPath()
        {
            EditorSettings editorSettingsAsset = EditorSettings.Get();
            string editorSettingsPath = AssetDatabase.GetAssetPath(editorSettingsAsset);
            string tempDirectory = Path.Combine(Path.GetDirectoryName(editorSettingsPath), "VistaTemp_CanIgnoreInVersionControl");
            if (!Directory.Exists(tempDirectory))
            {
                Directory.CreateDirectory(tempDirectory);
            }
            return tempDirectory;
        }
    }
}
#endif
