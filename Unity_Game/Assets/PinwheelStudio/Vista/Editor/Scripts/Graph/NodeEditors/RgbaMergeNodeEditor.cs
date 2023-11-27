#if VISTA
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(RgbaMergeNode))]
    public class RgbaMergeNodeEditor : ImageNodeEditorBase, INeedUpdateNodeVisual
    {
        private static readonly GUIContent MULTIPLIER_HEADER = new GUIContent("Multipliers");
        private static readonly GUIContent R_MUL = new GUIContent("R Multiplier", "Multiplier for R channel");
        private static readonly GUIContent G_MUL = new GUIContent("G Multiplier", "Multiplier for G channel");
        private static readonly GUIContent B_MUL = new GUIContent("B Multiplier", "Multiplier for B channel");
        private static readonly GUIContent A_MUL = new GUIContent("A Multiplier", "Multiplier for A channel");

        private static readonly GUIContent CUSTOM_LABELS_HEADER = new GUIContent("Custom Labels");
        private static readonly GUIContent TITLE_LABEL = new GUIContent("Title", "Custom title");
        private static readonly GUIContent R_LABEL = new GUIContent("R Label", "Custom label for R input");
        private static readonly GUIContent G_LABEL = new GUIContent("G Label", "Custom label for G input");
        private static readonly GUIContent B_LABEL = new GUIContent("B Label", "Custom label for B input");
        private static readonly GUIContent A_LABEL = new GUIContent("A Label", "Custom label for A input");
        private static readonly GUIContent SET_LABELS = new GUIContent("Set Labels");

        public class Labels
        {
            public string title { get; set; }
            public string rLabel { get; set; }
            public string gLabel { get; set; }
            public string bLabel { get; set; }
            public string aLabel { get; set; }
        }

        private static List<Labels> s_labels;
        public static List<Labels> labels
        {
            get
            {
                if (s_labels == null)
                {
                    s_labels = new List<Labels>();
                }
                return s_labels;
            }
        }

        public override void OnGUI(INode node)
        {
            RgbaMergeNode n = node as RgbaMergeNode;
            EditorGUI.BeginChangeCheck();
            EditorCommon.Header(MULTIPLIER_HEADER);
            float rm = EditorGUILayout.Slider(R_MUL, n.rMultiplier, 0f, 2f);
            float gm = EditorGUILayout.Slider(G_MUL, n.gMultiplier, 0f, 2f);
            float bm = EditorGUILayout.Slider(B_MUL, n.bMultiplier, 0f, 2f);
            float am = EditorGUILayout.Slider(A_MUL, n.aMultiplier, 0f, 2f);

            EditorCommon.Header(CUSTOM_LABELS_HEADER);
            string titleLabel = EditorGUILayout.DelayedTextField(TITLE_LABEL, n.titleLabel);
            string rLabel = EditorGUILayout.DelayedTextField(R_LABEL, n.rLabel);
            string gLabel = EditorGUILayout.DelayedTextField(G_LABEL, n.gLabel);
            string bLabel = EditorGUILayout.DelayedTextField(B_LABEL, n.bLabel);
            string aLabel = EditorGUILayout.DelayedTextField(A_LABEL, n.aLabel);
            
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.rMultiplier = rm;
                n.gMultiplier = gm;
                n.bMultiplier = bm;
                n.aMultiplier = am;

                n.titleLabel = titleLabel;
                n.rLabel = rLabel;
                n.gLabel = gLabel;
                n.bLabel = bLabel;
                n.aLabel = aLabel;
            }

            if (labels.Count != 0)
            {
                Rect r = EditorGUILayout.GetControlRect();
                if (GUI.Button(r, SET_LABELS))
                {
                    GenericMenu menu = new GenericMenu();
                    foreach (Labels l in labels)
                    {
                        menu.AddItem(
                            new GUIContent(l.title),
                            false,
                            () =>
                            {
                                m_graphEditor.RegisterUndo(n);
                                n.titleLabel = l.title;
                                n.rLabel = l.rLabel;
                                n.gLabel = l.gLabel;
                                n.bLabel = l.bLabel;
                                n.aLabel = l.aLabel;
                                m_graphEditor.UpdateNodesVisual();
                            });
                    }
                    menu.DropDown(r);
                }
            }
        }

        public void UpdateVisual(INode node, NodeView nv)
        {
            RgbaMergeNode n = node as RgbaMergeNode;
            List<PortView> ports = nv.inputContainer.Query<PortView>().ToList();
            if (ports.Count != 4)
            {
                return;
            }
            else
            {
                ports[0].portName = n.rLabel;
                ports[1].portName = n.gLabel;
                ports[2].portName = n.bLabel;
                ports[3].portName = n.aLabel;

                NodeMetadataAttribute meta = NodeMetadata.Get(typeof(RgbaMergeNode));
                if (meta != null && !string.IsNullOrEmpty(meta.title))
                {
                    if (!string.IsNullOrEmpty(n.titleLabel))
                    {
                        nv.title = $"{meta.title} ({n.titleLabel})";
                    }
                    else
                    {
                        nv.title = meta.title;
                    }
                }
                else
                {
                    nv.title = "RGBA Merge";
                }
            }

        }
    }
}
#endif
