#if VISTA
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.Compilation;
using UnityEngine.Networking;
using System;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;

namespace Pinwheel.VistaEditor.Graph
{
    public class PropertiesView : VisualElement
    {
        private GraphEditorBase m_editor;

        private VisualElement m_tabContainer;
        private VisualElement m_bodyContainer;
        private Button m_nodePropertiesTab;
        private Button m_graphPropertiesTab;
        private Button m_exploreTab;

        private IMGUIContainer m_nodePropertiesView;
        private IMGUIContainer m_graphPropertiesView;
        private IMGUIContainer m_exploreView;

        private static readonly string ACTIVE_TAB_PREF_KEY = "vista-graph-editor-properties-view-active-tab";
        private static int s_activeTabIndex = 2;

        public PropertiesView(GraphEditorBase editor) : base()
        {
            this.m_editor = editor;

            StyleSheet uss = Resources.Load<StyleSheet>("Vista/USS/Graph/PropertiesView");
            styleSheets.Add(uss);
            AddToClassList("properties-view");
            AddToClassList("panel");

            m_tabContainer = new VisualElement() { name = "tab-container" };
            this.Add(m_tabContainer);

            m_nodePropertiesTab = new Button() { text = "Node" };
            m_nodePropertiesTab.AddToClassList("tab");
            m_nodePropertiesTab.AddToClassList("active");
            m_nodePropertiesTab.clicked += OnNodePropertiesTabClicked;
            m_tabContainer.Add(m_nodePropertiesTab);

            m_graphPropertiesTab = new Button() { text = "Graph & Editor" };
            m_graphPropertiesTab.AddToClassList("tab");
            m_graphPropertiesTab.AddToClassList("active");
            m_graphPropertiesTab.clicked += OnGraphPropertiesTabClicked;
            m_tabContainer.Add(m_graphPropertiesTab);

            m_exploreTab = new Button() { text = "Explore <color=\"green\">•</color>" };
            m_exploreTab.AddToClassList("tab");
            m_exploreTab.AddToClassList("active");
            m_exploreTab.clicked += OnExploreTabClicked;
            m_tabContainer.Add(m_exploreTab);

            m_bodyContainer = new VisualElement() { name = "body" };
            this.Add(m_bodyContainer);

            ScrollView scrollView = new ScrollView(ScrollViewMode.VerticalAndHorizontal);
            scrollView.name = "scroll-view";
            m_bodyContainer.Add(scrollView);

            m_nodePropertiesView = new IMGUIContainer(OnNodePropertiesGUI);
            scrollView.Add(m_nodePropertiesView);

            m_graphPropertiesView = new IMGUIContainer(OnGraphPropertiesGUI);
            scrollView.Add(m_graphPropertiesView);

            m_exploreView = new IMGUIContainer(OnExploreGUI);
            scrollView.Add(m_exploreView);
        }

        public void OnEnable()
        {
            s_activeTabIndex = SessionState.GetInt(ACTIVE_TAB_PREF_KEY, 2);

            if (s_activeTabIndex == 0)
                OnNodePropertiesTabClicked();
            else if (s_activeTabIndex == 1)
                OnGraphPropertiesTabClicked();
            else
                OnExploreTabClicked();
        }

        public void OnDisable()
        {
            SessionState.SetInt(ACTIVE_TAB_PREF_KEY, s_activeTabIndex);
        }

        private void OnNodePropertiesTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", true);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            m_graphPropertiesTab.EnableInClassList("active", false);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_exploreTab.EnableInClassList("active", false);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            s_activeTabIndex = 0;
        }

        private void OnGraphPropertiesTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", false);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_graphPropertiesTab.EnableInClassList("active", true);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            m_exploreTab.EnableInClassList("active", false);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            s_activeTabIndex = 1;
        }

        private void OnExploreTabClicked()
        {
            m_nodePropertiesTab.EnableInClassList("active", false);
            m_nodePropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_graphPropertiesTab.EnableInClassList("active", false);
            m_graphPropertiesView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            m_exploreTab.EnableInClassList("active", true);
            m_exploreView.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);

            s_activeTabIndex = 2;
        }

        private void OnNodePropertiesGUI()
        {
            m_editor.OnDrawNodeProperties();
        }

        private void OnGraphPropertiesGUI()
        {
            m_editor.OnDrawGraphProperties();
        }

        private class ExploreGUI
        {
            private static readonly Color HIGHLIGHT_COLOR = new Color(1, 1, 1, 0.1F);
            private static readonly Color FADE_COLOR = new Color(0, 0, 0, 0.6F);
            private static readonly Color PROMOTION_TEXT_COLOR = new Color(1, 1, 0, 1f);

            private static readonly Texture CHECKMARK = Resources.Load<Texture>("Vista/Textures/Checkmark");

            public static void DrawNewsEntry(EditorSettings.MarketingSettings.NewsEntry e)
            {
                Rect entryRect = EditorGUILayout.BeginVertical();
                EditorGUIUtility.AddCursorRect(entryRect, MouseCursor.Link);
                if (entryRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(entryRect, HIGHLIGHT_COLOR);
                }
                EditorGUILayout.LabelField(e.title, EditorStyles.boldLabel, GUILayout.ExpandWidth(false));
                EditorGUILayout.LabelField(e.description, EditorCommon.Styles.grayMiniLabel);

                if (Event.current.type == EventType.MouseDown && entryRect.Contains(Event.current.mousePosition) && !string.IsNullOrEmpty(e.link))
                {
                    Application.OpenURL(e.link);
                }
                EditorGUILayout.EndVertical();
            }

            public static void DrawAssetEntry(EditorSettings.MarketingSettings.AssetEntry e)
            {
                GUI.enabled = !e.isInstalled;
                Rect entryRect = EditorGUILayout.BeginVertical();
                EditorGUIUtility.AddCursorRect(entryRect, MouseCursor.Link);
                if (entryRect.Contains(Event.current.mousePosition))
                {
                    EditorGUI.DrawRect(entryRect, HIGHLIGHT_COLOR);
                }

                EditorGUILayout.BeginHorizontal();
                Rect imageRect = EditorGUILayout.GetControlRect(GUILayout.Width(64), GUILayout.Height(64));
                imageRect = new RectOffset(4, 4, 4, 4).Remove(imageRect);
                GUI.DrawTexture(imageRect, e.texture != null ? e.texture : Texture2D.whiteTexture);
                if (e.isInstalled)
                {
                    EditorGUI.DrawRect(imageRect, FADE_COLOR);
                }

                EditorGUILayout.BeginVertical();
                string title = $"<b>{e.name}</b> <color=orange>{e.promotionText}</color>";
                EditorGUILayout.LabelField(title, EditorCommon.Styles.richTextLabel, GUILayout.ExpandWidth(false));
                EditorGUILayout.LabelField(e.description, EditorCommon.Styles.grayMiniLabel);
                if (e.isInstalled)
                {
                    EditorGUILayout.GetControlRect(GUILayout.Height(2));
                    Rect checkMarkRect = EditorGUILayout.GetControlRect(GUILayout.Height(12), GUILayout.Width(12));
                    GUI.DrawTexture(checkMarkRect, CHECKMARK);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                GUI.enabled = true;

                if (Event.current.type == EventType.MouseDown && entryRect.Contains(Event.current.mousePosition) && !string.IsNullOrEmpty(e.link))
                {
                    Application.OpenURL(e.link + Links.AFF);
                }
            }

            public static readonly GUIContent NEWS = new GUIContent("NEWS");
            public static readonly GUIContent COMPLETE_YOUR_COLLECTION = new GUIContent("COMPLETE YOUR VISTA COLLECTION");
            public static readonly GUIContent OTHER_PRODUCTS_FROM_PINWHEEL = new GUIContent("OTHER PRODUCTS FROM PINWHEEL STUDIO");
        }

        private void OnExploreGUI()
        {
            EditorSettings.MarketingSettings marketing = EditorSettings.Get().marketingSettings;
            EditorSettings.MarketingSettings.NewsEntry[] news = marketing.GetNews();
            if (news.Length > 0)
            {
                EditorCommon.Header(ExploreGUI.NEWS);
                foreach (EditorSettings.MarketingSettings.NewsEntry e in news)
                {
                    ExploreGUI.DrawNewsEntry(e);
                }
            }

            EditorSettings.MarketingSettings.AssetEntry[] vistaModules = marketing.GetVistaModules();
            if (vistaModules.Length > 0)
            {
                EditorCommon.Header(ExploreGUI.COMPLETE_YOUR_COLLECTION);
                foreach (EditorSettings.MarketingSettings.AssetEntry e in vistaModules)
                {
                    ExploreGUI.DrawAssetEntry(e);
                }
            }

            EditorSettings.MarketingSettings.AssetEntry[] featuredAssets = marketing.GetFeaturedAssets();
            if (featuredAssets.Length > 0)
            {
                EditorCommon.Header(ExploreGUI.OTHER_PRODUCTS_FROM_PINWHEEL);
                foreach (EditorSettings.MarketingSettings.AssetEntry e in featuredAssets)
                {
                    ExploreGUI.DrawAssetEntry(e);
                }
            }
        }
    }
}
#endif
