#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [InitializeOnLoad]
    public abstract class NodeEditor
    {
        internal delegate void ExposedPropertiesGuiHandler(GraphAsset parentGraph, INode node);
        internal static event ExposedPropertiesGuiHandler exposedPropertiesGuiCallback;

        public GraphEditorBase m_graphEditor;

        protected static readonly GUIContent ID = new GUIContent("Id", "Id of the node in this graph");

        public virtual bool hasSpecificProperties
        {
            get
            {
                return true;
            }
        }

        public virtual bool needConstantUpdate2D
        {
            get
            {
                return false;
            }
        }

        public virtual void OnBaseGUI(INode node)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(ID);
            EditorGUILayout.SelectableLabel(node.id.ToString(), GUILayout.Height(EditorGUIUtility.singleLineHeight));
            EditorGUILayout.EndHorizontal();
        }

        public abstract void OnGUI(INode node);

        public virtual void OnViewport2dGUI(INode node, Rect imguiRect, Rect imageRect) { }

        internal void OnExposedPropertiesGUI(INode node)
        {
            exposedPropertiesGuiCallback?.Invoke(m_graphEditor.clonedGraph, node);
        }

        private static Dictionary<Type, Type> editorTypeMap;

        [InitializeOnLoadMethod]
        private static void CacheEditors()
        {
            editorTypeMap = new Dictionary<Type, Type>();
            List<Type> editorTypes = TypeCache.GetTypesDerivedFrom<NodeEditor>().ToList();
            foreach (Type t in editorTypes)
            {
                NodeEditorAttribute att = t.GetCustomAttribute<NodeEditorAttribute>();
                if (att != null && att.nodeType != null)
                {
                    editorTypeMap[att.nodeType] = t;
                }
            }
        }

        public static NodeEditor Get<T>() where T : INode
        {
            Type nodeType = typeof(T);
            return Get(nodeType);
        }

        public static NodeEditor Get(Type nodeType)
        {
            if (!typeof(INode).IsAssignableFrom(nodeType))
                throw new ArgumentException("nodeType must implement INode");
            Type editorType;
            if (editorTypeMap.TryGetValue(nodeType, out editorType))
            {
                return Activator.CreateInstance(editorType) as NodeEditor;
            }
            else
            {
                return null;
            }
        }
    }
}
#endif
