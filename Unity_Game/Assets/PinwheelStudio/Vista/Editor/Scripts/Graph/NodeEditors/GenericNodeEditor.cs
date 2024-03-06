#if VISTA
using Pinwheel.Vista.Graph;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    public class GenericNodeEditor : NodeEditor
    {
        public override void OnGUI(INode node)
        {
            List<FieldInfo> fields = GetSerializableFields(node);
            foreach (FieldInfo f in fields)
            {
                DrawField(node, f);
            }
        }

        private List<FieldInfo> GetSerializableFields(INode node)
        {
            List<FieldInfo> serializableFields = new List<FieldInfo>();

            FieldInfo[] fields = node.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (FieldInfo f in fields)
            {
                if (IsSerializable(f))
                {
                    serializableFields.Add(f);
                }
            }
            return serializableFields;
        }

        private bool IsSerializable(FieldInfo f)
        {
            if (f.IsInitOnly)
            {
                return false;
            }
            if (f.GetCustomAttribute<HideInInspector>() != null)
            {
                return false;
            }
            if (f.IsPublic && f.GetCustomAttribute<System.NonSerializedAttribute>() == null)
            {
                return true;
            }
            else if (!f.IsPublic && f.GetCustomAttribute<SerializeField>() != null)
            {
                return true;
            }
            return false;
        }

        private void DrawField(INode node, FieldInfo f)
        {
            object value = f.GetValue(node);
            string label = f.Name;

            EditorGUI.BeginChangeCheck();
            if (f.FieldType == typeof(int))
            {
                value = EditorGUILayout.IntField(label, (int)value);
            }
            else if (f.FieldType == typeof(float))
            {
                value = EditorGUILayout.FloatField(label, (float)value);
            }
            else if (f.FieldType == typeof(string))
            {
                value = EditorGUILayout.TextField(label, (string)value);
            }
            else if (f.FieldType == typeof(bool))
            {
                value = EditorGUILayout.Toggle(label, (bool)value);
            }
            else if (f.FieldType == typeof(Vector2))
            {
                value = EditorGUILayout.Vector2Field(label, (Vector2)value);
            }
            else if (f.FieldType == typeof(Vector2Int))
            {
                value = EditorGUILayout.Vector2IntField(label, (Vector2Int)value);
            }
            else if (f.FieldType == typeof(Vector3))
            {
                value = EditorGUILayout.Vector3Field(label, (Vector3)value);
            }
            else if (f.FieldType == typeof(Vector3Int))
            {
                value = EditorGUILayout.Vector3IntField(label, (Vector3Int)value);
            }
            else if (f.FieldType == typeof(Vector4))
            {
                value = EditorGUILayout.Vector4Field(label, (Vector4)value);
            }
            else if (f.FieldType == typeof(AnimationCurve))
            {
                value = EditorGUILayout.CurveField(label, (AnimationCurve)value);
            }
            else if (f.FieldType == typeof(Gradient))
            {
                value = EditorGUILayout.GradientField(label, (Gradient)value);
            }
            else if (f.FieldType == typeof(Color))
            {
                value = EditorGUILayout.ColorField(label, (Color)value);
            }
            else if (f.FieldType == typeof(Color32))
            {
                value = EditorGUILayout.ColorField(label, (Color32)value);
            }
            else if (f.FieldType == typeof(Rect))
            {
                value = EditorGUILayout.RectField(label, (Rect)value);
            }
            else if (f.FieldType.IsSubclassOf(typeof(UnityEngine.Object)))
            {
                value = EditorGUILayout.ObjectField(label, (UnityEngine.Object)value, f.FieldType, true);
            }
            else if (f.FieldType.IsEnum)
            {
                value = EditorGUILayout.EnumPopup(label, (Enum)value);
            }
            else
            {
                EditorGUILayout.LabelField(string.Format("Field {0} of type {1}", label, f.FieldType.Name));
            }

            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(node);
                f.SetValue(node, value);
            }
        }
    }
}
#endif
