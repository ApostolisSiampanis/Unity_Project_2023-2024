#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(SelectPositionNode))]
    public class SelectPositionNodeEditor : ExecutableNodeEditorBase
    {
        private static readonly GUIContent EDIT_POSITIONS = new GUIContent("Edit Positions");
        private static readonly GUIContent HELP = new GUIContent(
            "Use 2D View to edit points:\n" +
            "- Shift Click to add.\n" +
            "- Ctrl Click to remove.\n" +
            "- Drag Left Mouse to move.");

        public override void OnGUI(INode node)
        {
            EditorGUILayout.HelpBox(HELP.text, MessageType.Info, true);
        }

        public override void OnViewport2dGUI(INode node, Rect imguiRect, Rect imageRect)
        {
            SelectPositionNode n = node as SelectPositionNode;
            EditorGUI.BeginChangeCheck();
            Vector2[] positions = PositionEditor(imguiRect, imageRect, n.positions);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.positions = positions;
            }
        }

        private class PositionEditorInternal
        {
            public static readonly Color32 backgroundColor = new Color32(0, 0, 0, 255);

            public static readonly Color pointHandleColor = new Color32(255, 255, 255, 255);
            public static readonly Vector2 pointHandleSize = new Vector2(10, 10);
            public static readonly Vector2 pointHandlePickingSize = new Vector2(25, 25);

            public static int selectedHandleIndex = -1;
            public static bool isDragging = false;

            public static Rect GetHandleRect(Rect bounds, Vector2 p)
            {
                Rect r = new Rect();
                r.size = pointHandlePickingSize;
                r.center = Utilities.NormalizedToPoint(bounds, new Vector2(p.x, 1 - p.y));
                return r;
            }

            public static Rect[] GetHandleRects(Rect bounds, List<Vector2> points)
            {
                Rect[] rects = new Rect[points.Count];
                for (int i = 0; i < points.Count; ++i)
                {
                    rects[i] = GetHandleRect(bounds, points[i]);
                }
                return rects;
            }

            public static void DrawHandles(Rect[] pRects)
            {
                for (int i = 0; i < pRects.Length; ++i)
                {
                    Color c = i == selectedHandleIndex ? Handles.selectedColor : pointHandleColor;
                    EditorGUI.DrawRect(new Rect() { size = pointHandleSize, center = pRects[i].center }, c);
                }
            }

            public static void AddPoint(List<Vector2> points, Vector2 p)
            {
                int insertIndex = GetInsertIndex(points, p);

                if (insertIndex >= 0 && insertIndex < points.Count)
                {
                    points.Insert(insertIndex, p);
                }
                else
                {
                    points.Add(p);
                }
            }

            public static void Drag(Rect bounds, List<Vector2> points, Vector2 mousePosition)
            {
                Vector2 p = Utilities.PointToNormalized(bounds, mousePosition);
                p.y = 1 - p.y;
                points[selectedHandleIndex] = p;
            }

            public static int GetInsertIndex(List<Vector2> points, Vector2 p)
            {
                int insertIndex = -1;
                if (points.Count < 2)
                {
                    return insertIndex;
                }

                float d = 0;
                float minDistance = float.MaxValue;
                Vector2 center;

                center = (points[0] + points[points.Count - 1]) * 0.5f;
                d = Vector2.Distance(p, center);
                if (d < minDistance)
                {
                    minDistance = d;
                    insertIndex = 0;
                }

                for (int i = 1; i < points.Count; ++i)
                {
                    center = (points[i] + points[i - 1]) * 0.5f;
                    d = Vector2.Distance(p, center);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        insertIndex = i;
                    }
                }

                return insertIndex;
            }
        }

        public static Vector2[] PositionEditor(Rect imguiRect, Rect imageRect, Vector2[] polygon)
        {
            List<Vector2> points = new List<Vector2>();
            points.AddRange(polygon);

            Rect r = imageRect;
            EditorGUI.DrawRect(r, PositionEditorInternal.backgroundColor);

            Rect[] pRects = PositionEditorInternal.GetHandleRects(r, points);
            PositionEditorInternal.DrawHandles(pRects);

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    if (Event.current.shift)
                    {
                        Vector2 newPoint = Utilities.PointToNormalized(r, Event.current.mousePosition);
                        newPoint.y = 1 - newPoint.y;
                        PositionEditorInternal.AddPoint(points, newPoint);
                        GUI.changed = true;
                    }
                    else if (Event.current.control)
                    {
                        for (int i = 0; i < pRects.Length; ++i)
                        {
                            if (pRects[i].Contains(Event.current.mousePosition))
                            {
                                points.RemoveAt(i);
                                GUI.changed = true;
                            }
                        }
                    }
                    else
                    {
                        PositionEditorInternal.selectedHandleIndex = -1;
                        for (int i = 0; i < pRects.Length; ++i)
                        {
                            if (pRects[i].Contains(Event.current.mousePosition))
                            {
                                PositionEditorInternal.selectedHandleIndex = i;
                                PositionEditorInternal.isDragging = true;
                            }
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (PositionEditorInternal.isDragging &&
                    PositionEditorInternal.selectedHandleIndex >= 0 &&
                    PositionEditorInternal.selectedHandleIndex < points.Count)
                {
                    PositionEditorInternal.Drag(r, points, Event.current.mousePosition);
                    GUI.changed = true;
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                PositionEditorInternal.isDragging = false;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                bool willDeselect = true;
                for (int i = 0; i < pRects.Length; ++i)
                {
                    if (pRects[i].Contains(Event.current.mousePosition))
                    {
                        willDeselect = false;
                        break;
                    }
                }
                if (willDeselect)
                {
                    PositionEditorInternal.selectedHandleIndex = -1;
                    PositionEditorInternal.isDragging = false;
                }
            }
            return points.ToArray();
        }
    }
}
#endif
