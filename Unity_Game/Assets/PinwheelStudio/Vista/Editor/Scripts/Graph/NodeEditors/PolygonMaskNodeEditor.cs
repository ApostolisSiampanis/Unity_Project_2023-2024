#if VISTA
using Pinwheel.Vista;
using Pinwheel.Vista.Graph;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor.Graph
{
    [NodeEditor(typeof(PolygonMaskNode))]
    public class PolygonMaskNodeEditor : ImageNodeEditorBase
    {
        private static readonly GUIContent FALLOFF = new GUIContent("Falloff", "Define the size of the falloff region");
        private static readonly GUIContent HELP = new GUIContent(
             "Use 2D View to edit points:\n" +
             "- Shift Click to add.\n" +
             "- Ctrl Click to remove.\n" +
             "- Drag Left Mouse to move.");

        public override void OnGUI(INode node)
        {
            PolygonMaskNode n = node as PolygonMaskNode;
            EditorGUI.BeginChangeCheck();
            float falloff = EditorGUILayout.Slider(FALLOFF, n.falloff, 0f, 1f);
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.falloff = falloff;
            }
            EditorGUILayout.HelpBox(HELP.text, MessageType.Info, true);
        }

        public override void OnViewport2dGUI(INode node, Rect imguiRect, Rect imageRect)
        {
            PolygonMaskNode n = node as PolygonMaskNode;
            EditorGUI.BeginChangeCheck();
            Vector2[] polygon = PolygonEditor(imguiRect, imageRect, n.vertices, n.GetFalloffVertices());
            if (EditorGUI.EndChangeCheck())
            {
                m_graphEditor.RegisterUndo(n);
                n.vertices = polygon;
            }
        }

        private class PolygonEditorInternal
        {
            public static readonly Color pointHandleColor = new Color32(255, 255, 255, 255);
            public static readonly Color falloffColor = new Color32(255, 255, 255, 127);
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
                if (pRects.Length > 1)
                {
                    Handles.BeginGUI();
                    Handles.color = pointHandleColor;
                    for (int i = 0; i < pRects.Length - 1; ++i)
                    {
                        Handles.DrawAAPolyLine(3, pRects[i].center, pRects[i + 1].center);
                    }
                    Handles.DrawAAPolyLine(3, pRects[0].center, pRects[pRects.Length - 1].center);
                    Handles.EndGUI();
                }

                for (int i = 0; i < pRects.Length; ++i)
                {
                    Color c = i == selectedHandleIndex ? Handles.selectedColor : pointHandleColor;
                    EditorGUI.DrawRect(new Rect() { size = pointHandleSize, center = pRects[i].center }, c);
                }
            }

            public static void DrawFalloff(Rect bounds, Vector2[] falloff)
            {
                if (falloff.Length >= 3)
                {
                    Handles.BeginGUI();
                    Handles.color = falloffColor;
                    Rect r0, r1;
                    for (int i = 0; i < falloff.Length - 1; ++i)
                    {
                        r0 = GetHandleRect(bounds, falloff[i]);
                        r1 = GetHandleRect(bounds, falloff[i + 1]);
                        Handles.DrawAAPolyLine(3, r0.center, r1.center);
                    }
                    r0 = GetHandleRect(bounds, falloff[0]);
                    r1 = GetHandleRect(bounds, falloff[falloff.Length - 1]);
                    Handles.DrawAAPolyLine(3, r0.center, r1.center);
                    Handles.EndGUI();
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

        public static Vector2[] PolygonEditor(Rect imguiRect, Rect imageRect, Vector2[] polygon, Vector2[] fallofftedPolygon)
        {
            List<Vector2> points = new List<Vector2>();
            points.AddRange(polygon);

            Rect r = imageRect;

            PolygonEditorInternal.DrawFalloff(r, fallofftedPolygon);
            Rect[] pRects = PolygonEditorInternal.GetHandleRects(r, points);
            PolygonEditorInternal.DrawHandles(pRects);

            if (Event.current.type == EventType.MouseDown)
            {
                if (Event.current.button == 0)
                {
                    if (Event.current.shift)
                    {
                        Vector2 newPoint = Utilities.PointToNormalized(r, Event.current.mousePosition);
                        newPoint.y = 1 - newPoint.y;
                        PolygonEditorInternal.AddPoint(points, newPoint);
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
                        PolygonEditorInternal.selectedHandleIndex = -1;
                        for (int i = 0; i < pRects.Length; ++i)
                        {
                            if (pRects[i].Contains(Event.current.mousePosition))
                            {
                                PolygonEditorInternal.selectedHandleIndex = i;
                                PolygonEditorInternal.isDragging = true;
                            }
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (PolygonEditorInternal.isDragging &&
                    PolygonEditorInternal.selectedHandleIndex >= 0 &&
                    PolygonEditorInternal.selectedHandleIndex < points.Count)
                {
                    PolygonEditorInternal.Drag(r, points, Event.current.mousePosition);
                    GUI.changed = true;
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                PolygonEditorInternal.isDragging = false;
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
                    PolygonEditorInternal.selectedHandleIndex = -1;
                    PolygonEditorInternal.isDragging = false;
                }
            }
            return points.ToArray();
        }
    }
}
#endif
