#if VISTA
using Pinwheel.Vista.Graph;
using Pinwheel.VistaEditor.UIElements;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.VistaEditor.Graph
{
    public class TerrainGraphViewport2d : Viewport
    {
        public delegate void AddUtilityButtonHandler(TerrainGraphViewport2d graphView, List<UtilityButton> buttons);
        public static event AddUtilityButtonHandler addUtilityButtonCallback;

        internal TerrainGraphEditor m_editor;

        private TextElement m_pixelInfoText;
        private TextElement m_textureInfoText;
        private IMGUIContainer m_imguiContainer;

        private VisualElement m_buttonContainer;
        private UtilityButton m_takeScreenshotButton;

        private static readonly string KEY_VISIBLE = "visible";
        private static readonly string KEY_WIDTH = "width";

        private const int IMAGE_MARGIN = 64;

        private ComputeShader m_getPixelShader;
        private RenderTexture m_targetRt;
        private Material m_material;

        private static readonly string VIS_2D_SHADER_NAME = "Hidden/Vista/TerrainGraphEditor/Visualize2D";
        private static readonly int MAIN_TEX = Shader.PropertyToID("_MainTex");
        private static readonly int GRADIENT_TEX = Shader.PropertyToID("_GradientTex");

        private class ViewportImageInfo
        {
            public RenderTexture renderTexture { get; set; }
            public string infoText { get; set; }
        }

        public TerrainGraphViewport2d(TerrainGraphEditor editor) : base()
        {
            m_editor = editor;

            m_pixelInfoText = new TextElement();
            m_pixelInfoText.style.position = new StyleEnum<Position>(Position.Absolute);
            m_pixelInfoText.style.left = new StyleLength(0f);
            m_pixelInfoText.style.right = new StyleLength(0f);
            m_pixelInfoText.style.bottom = new StyleLength(32f);
            m_pixelInfoText.style.height = new StyleLength(StyleKeyword.Auto);
            m_pixelInfoText.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            m_pixelInfoText.pickingMode = PickingMode.Ignore;
            this.Add(m_pixelInfoText);

            m_textureInfoText = new TextElement();
            m_textureInfoText.style.position = new StyleEnum<Position>(Position.Absolute);
            m_textureInfoText.style.left = new StyleLength(0f);
            m_textureInfoText.style.right = new StyleLength(0f);
            m_textureInfoText.style.bottom = new StyleLength(10f);
            m_textureInfoText.style.height = new StyleLength(StyleKeyword.Auto);
            m_textureInfoText.style.unityTextAlign = new StyleEnum<TextAnchor>(TextAnchor.MiddleCenter);
            m_textureInfoText.pickingMode = PickingMode.Ignore;
            this.Add(m_textureInfoText);

            m_imguiContainer = new IMGUIContainer() { name = "2d-imgui" };
            m_imguiContainer.onGUIHandler += OnIMGUI;
            m_imguiContainer.style.position = new StyleEnum<Position>(Position.Absolute);
            m_imguiContainer.style.left = new StyleLength(0f);
            m_imguiContainer.style.top = new StyleLength(0f);
            m_imguiContainer.style.right = new StyleLength(0f);
            m_imguiContainer.style.bottom = new StyleLength(0f);
            this.Add(m_imguiContainer);

            m_buttonContainer = new VisualElement() { name = "utility-button-container" };
            m_buttonContainer.AddToClassList("utility-button-container");
            Add(m_buttonContainer);

            m_takeScreenshotButton = new UtilityButton() { name = "take-screenshot-button" };
            m_takeScreenshotButton.image = Resources.Load<Texture2D>("Vista/Textures/TakeScreenshot");
            m_takeScreenshotButton.tooltip = "Take screenshot";
            m_takeScreenshotButton.clicked += OnTakeScreenshotButtonClicked;
            m_buttonContainer.Add(m_takeScreenshotButton);

            AddToClassList("viewport-2d");
            AddToClassList("panel");

            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);

            if (addUtilityButtonCallback != null)
            {
                List<UtilityButton> additionalButtons = new List<UtilityButton>();
                addUtilityButtonCallback.Invoke(this, additionalButtons);
                foreach (UtilityButton b in additionalButtons)
                {
                    if (b != null)
                    {
                        m_buttonContainer.Add(b);
                    }
                }
            }

            m_getPixelShader = Resources.Load<ComputeShader>("Vista/Shaders/GetPixel");
        }

        public void OnEnable()
        {
            string typeName = this.GetType().Name;
            bool v = EditorPrefs.GetBool(typeName + KEY_VISIBLE, true);
            style.display = new StyleEnum<DisplayStyle>(v ? DisplayStyle.Flex : DisplayStyle.None);

            float w = EditorPrefs.GetFloat(typeName + KEY_WIDTH, -1);
            if (w > 0)
            {
                style.width = new StyleLength(w);
            }
            else
            {
                style.width = new StyleLength(new Length(50, LengthUnit.Percent));
            }
        }

        public void OnDisable()
        {
            string typeName = this.GetType().Name;
            if (resolvedStyle != null)
            {
                bool v = resolvedStyle.display == DisplayStyle.Flex ? true : false;
                EditorPrefs.SetBool(typeName + KEY_VISIBLE, v);

                float w = resolvedStyle.width;
                EditorPrefs.SetFloat(typeName + KEY_WIDTH, w);
            }

            CleanUp();
        }

        public void RenderViewport()
        {
            if (m_editor.clonedGraph == null)
                return;

            ViewportImageInfo info = GetViewportImageInfo();

            if (info == null || info.renderTexture == null)
            {
                image = Texture2D.blackTexture;
                m_textureInfoText.text = "";
            }
            else
            {
                if (info.renderTexture.format == RenderTextureFormat.ARGB32)
                {
                    image = info.renderTexture;
                    m_textureInfoText.text = info.infoText;
                }
                else
                {
                    EditorSettings editorSettings = EditorSettings.Get();

                    if (m_targetRt == null)
                    {
                        int rtWidth = Mathf.Max(1, info.renderTexture.width);
                        int rtHeight = Mathf.Max(1, info.renderTexture.width);
                        m_targetRt = new RenderTexture(rtWidth, rtHeight, 16, RenderTextureFormat.ARGB32);
                    }
                    if (m_material == null)
                    {
                        m_material = new Material(Shader.Find(VIS_2D_SHADER_NAME));
                    }
                    m_material.SetTexture(MAIN_TEX, info.renderTexture);
                    m_material.SetTexture(GRADIENT_TEX, editorSettings.graphEditorSettings.GetViewportGradient());
                    Drawing.DrawQuad(m_targetRt, m_material, 0);

                    image = m_targetRt;
                    m_textureInfoText.text = info.infoText;
                }
            }
        }

        private void CleanUp()
        {
            if (m_targetRt != null)
            {
                m_targetRt.Release();
                m_targetRt = null;
            }
            if (m_material != null)
            {
                UnityEngine.Object.DestroyImmediate(m_material);
            }
        }

        private ViewportImageInfo GetViewportImageInfo()
        {
            ViewportImageInfo info = new ViewportImageInfo();

            if (m_editor.m_lastExecution != null)
            {
                INode n = null;
                n = m_editor.clonedGraph.GetNode(m_editor.m_display2dNodeId);
                if (n == null)
                {
                    n = m_editor.clonedGraph.GetNode(m_editor.m_activeNodeId);
                }

                if (n != null)
                {
                    ISlot[] outputSlot = n.GetOutputSlots();
                    if (outputSlot.Length == 0)
                    {
                        info.renderTexture = null;
                    }
                    else
                    {
                        string textureName = DataPool.GetName(n.id, outputSlot[0].id);
                        info.renderTexture = m_editor.m_lastExecution.data.GetRT(textureName);

                        if (info.renderTexture != null)
                        {
                            NodeMetadataAttribute meta = NodeMetadata.Get(n.GetType());
                            string nodeTitle;
                            if (meta != null && !string.IsNullOrEmpty(meta.title))
                            {
                                nodeTitle = meta.title;
                            }
                            else
                            {
                                nodeTitle = ObjectNames.NicifyVariableName(n.GetType().Name);
                            }

                            string slotName = outputSlot[0].name;
                            info.infoText = $"{nodeTitle} | {slotName} | {info.renderTexture.width}x{info.renderTexture.height} | {info.renderTexture.format}";
                        }
                    }
                }
            }
            return info;
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            if (m_image != null)
            {
                m_image.style.position = new StyleEnum<Position>(Position.Absolute);
                Rect r = evt.newRect;
                if (r.width == r.height)
                {
                    m_image.style.left = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.top = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.right = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.bottom = new StyleLength(0f + IMAGE_MARGIN);
                }
                else if (r.width > r.height)
                {
                    m_image.style.left = new StyleLength((r.width - r.height) * 0.5f + IMAGE_MARGIN);
                    m_image.style.top = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.right = new StyleLength((r.width - r.height) * 0.5f + IMAGE_MARGIN);
                    m_image.style.bottom = new StyleLength(0f + IMAGE_MARGIN);
                }
                else if (r.width < r.height)
                {
                    m_image.style.left = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.top = new StyleLength((r.height - r.width) * 0.5f + IMAGE_MARGIN);
                    m_image.style.right = new StyleLength(0f + IMAGE_MARGIN);
                    m_image.style.bottom = new StyleLength((r.height - r.width) * 0.5f + IMAGE_MARGIN);
                }
            }
        }

        private void OnIMGUI()
        {
            if (m_imguiContainer.resolvedStyle == null)
                return;
            Rect imguiRect = new Rect(0, 0, m_imguiContainer.resolvedStyle.width, m_imguiContainer.resolvedStyle.height);
            if (m_image.resolvedStyle == null)
                return;
            Rect imageRect = new Rect(m_image.resolvedStyle.left, m_image.resolvedStyle.top, m_image.resolvedStyle.width, m_image.resolvedStyle.height);
            m_editor.OnViewport2dIMGUI(imguiRect, imageRect);

            if (imageRect.Contains(Event.current.mousePosition))
            {
                Vector2 uv = Rect.PointToNormalized(imageRect, Event.current.mousePosition);
                uv.y = 1 - uv.y;
                UpdatePixelInfo(uv);
            }
            else
            {
                m_pixelInfoText.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                m_pixelInfoText.text = "";
            }
        }

        private static class GetPixelConfig
        {
            public static readonly int PIXEL_POS = Shader.PropertyToID("_PixelPos");
            public static readonly int TEXTURE = Shader.PropertyToID("_Texture");
            public static readonly int OUTPUT = Shader.PropertyToID("_Output");

            public static readonly int KERNEL = 0;
        }

        private void UpdatePixelInfo(Vector2 uv)
        {
            if (image != null && m_getPixelShader != null)
            {
                m_pixelInfoText.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                Vector2 pixelPos = new Vector2(
                    Mathf.FloorToInt(uv.x * (image.width - 1)),
                    Mathf.FloorToInt(uv.y * (image.height - 1)));
                ComputeBuffer outputBuffer = new ComputeBuffer(1, sizeof(float) * 4, ComputeBufferType.Structured);

                m_getPixelShader.SetVector(GetPixelConfig.PIXEL_POS, pixelPos);
                m_getPixelShader.SetTexture(GetPixelConfig.KERNEL, GetPixelConfig.TEXTURE, image);
                m_getPixelShader.SetBuffer(GetPixelConfig.KERNEL, GetPixelConfig.OUTPUT, outputBuffer);

                m_getPixelShader.Dispatch(GetPixelConfig.KERNEL, 1, 1, 1);

                Vector4[] outputData = new Vector4[1];
                outputBuffer.GetData(outputData);
                outputBuffer.Release();

                m_pixelInfoText.text = $"Pixel value: {outputData[0].ToString("0.000")}";
            }
            else
            {
                m_pixelInfoText.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
                m_pixelInfoText.text = "";
            }
        }

        private void OnTakeScreenshotButtonClicked()
        {
            string folder = "Assets/VistaScreenshots/";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            string fileName = $"{m_editor.sourceGraph.name}_2D_{DateTime.Now.Ticks}.png";
            string fullPath = Path.Combine(folder, fileName);
            EditorCoroutineUtility.StartCoroutine(DoTakeScreenshot(fullPath), this);
        }

        private IEnumerator DoTakeScreenshot(string fullPath)
        {
            m_buttonContainer.visible = false;
            yield return null;

            Rect r = this.worldBound;
            r.position += m_editor.position.position;

            Texture2D tex = EditorScreenshot.Capture(r.position, (int)r.width, (int)r.height);
            byte[] data = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, data);
            UnityEngine.Object.DestroyImmediate(tex);
            AssetDatabase.Refresh();

            yield return null;
            m_buttonContainer.visible = true;
            Texture2D assetTex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            EditorGUIUtility.PingObject(assetTex);
            Selection.activeObject = assetTex;
            AssetDatabase.OpenAsset(assetTex);
            Debug.Log($"Screenshot saved at {fullPath}", assetTex);
        }
    }
}
#endif
