#if VISTA
using Pinwheel.Vista.Graph;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public class ImageNodeEditorBase : GenericNodeEditor
    {
        private static readonly GUIContent RES_OVERRIDE = new GUIContent("Resolution", "Decide the texture's resolution");
        private static readonly GUIContent RES_MULTIPLIER = new GUIContent(" ", "Relative size of the texture compare it the graph settings");
        private static readonly GUIContent RES_ABSOLUTE = new GUIContent(" ", "Size of the texture in pixel");
        private static readonly GUIContent SPLIT_EXECUTION = new GUIContent("Split Execution", "Split the node execution to several frames to reduce hiccup");
        private static readonly GUIContent BYPASS = new GUIContent("Bypass", "Ignore the execution of this node");

        public override void OnBaseGUI(INode node)
        {
            base.OnBaseGUI(node);
            ImageNodeBase n = node as ImageNodeBase;
            ResolutionOverrideOptions resolutionOverride = n.resolutionOverride;
            float resolutionMultiplier = n.resolutionMultiplier;
            int resolutionAbsolute = n.resolutionAbsolute;
            bool splitExecution = n.shouldSplitExecution;
            bool bypass = n.isBypassed;

            EditorGUI.BeginChangeCheck();
            resolutionOverride = (ResolutionOverrideOptions)EditorGUILayout.EnumPopup(RES_OVERRIDE, n.resolutionOverride);
            if (resolutionOverride == ResolutionOverrideOptions.RelativeToGraph || resolutionOverride == ResolutionOverrideOptions.RelativeToMainInput)
            {
                resolutionMultiplier = EditorGUILayout.Slider(RES_MULTIPLIER, n.resolutionMultiplier, 0.1f, 2f);
            }
            else if (resolutionOverride == ResolutionOverrideOptions.Absolute)
            {
                resolutionAbsolute = EditorGUILayout.IntField(RES_ABSOLUTE, n.resolutionAbsolute);
            }
            splitExecution = EditorGUILayout.Toggle(SPLIT_EXECUTION, n.shouldSplitExecution);

            if (!(n is IOutputNode))
            {
                bypass = EditorGUILayout.Toggle(BYPASS, n.isBypassed);
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.resolutionOverride = resolutionOverride;
                n.resolutionMultiplier = resolutionMultiplier;
                n.resolutionAbsolute = resolutionAbsolute;
                n.shouldSplitExecution = splitExecution;
                n.isBypassed = bypass;
            }
        }
    }
}
#endif
