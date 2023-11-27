#if VISTA
using System;
using UnityEditor;
using UnityEngine;

namespace Pinwheel.VistaEditor
{
    public class PointsDragger
    {
        public struct Arguments
        {
            public Action<Vector2> pointGizmosFunction;
            public Action<Vector2[]> drawConnectorFunction;
            public Color32 gizmosColor;

            public static Arguments Create()
            {
                Arguments args = new Arguments()
                {
                    pointGizmosFunction = null,
                    drawConnectorFunction = null,
                    gizmosColor = new Color32(255, 255, 255, 255)
                };
                return args;
            }
        }

        private static readonly float HANDLE_SIZE = 10;
        private static int draggedPointsIndex = -1;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="background"></param>
        /// <param name="points">In UV space</param>
        public static void DrawCanvas(Rect imguiRect, Rect imageRect, Arguments args, Vector2[] points)
        {
            Handles.BeginGUI();
            Color color = Handles.color;
            Handles.color = args.gizmosColor;
            HandlePointDragging(imageRect, args, points);
            Handles.color = color;
            Handles.EndGUI();
        }

        private static Vector2 FlipY(Vector2 v)
        {
            return new Vector2(v.x, 1 - v.y);
        }

        private static void HandlePointDragging(Rect imageRect, Arguments args, Vector2[] points)
        {
            Vector2[] uvSpaceCorners = new Vector2[points.Length];
            for (int i = 0; i < uvSpaceCorners.Length; ++i)
            {
                uvSpaceCorners[i] = points[i];
            }

            Matrix4x4 uvToCanvas = Matrix4x4.TRS(new Vector3(imageRect.position.x, imageRect.position.y, 0), Quaternion.identity, new Vector3(imageRect.width, imageRect.height, 1));
            Vector2[] canvasSpaceCorners = new Vector2[points.Length];
            Rect[] cornersHandleRect = new Rect[points.Length];
            for (int i = 0; i < uvSpaceCorners.Length; ++i)
            {
                canvasSpaceCorners[i] = uvToCanvas.MultiplyPoint(FlipY(uvSpaceCorners[i]));
                cornersHandleRect[i] = new Rect() { size = Vector2.one * HANDLE_SIZE, center = canvasSpaceCorners[i] };
            }

            for (int i = 0; i < canvasSpaceCorners.Length; ++i)
            {
                if (args.pointGizmosFunction != null)
                {
                    args.pointGizmosFunction.Invoke(canvasSpaceCorners[i]);
                }
                else
                {
                    EditorGUI.DrawRect(cornersHandleRect[i], draggedPointsIndex == i ? Handles.selectedColor : (Color)args.gizmosColor);
                }
            }

            if (args.drawConnectorFunction != null)
            {
                args.drawConnectorFunction.Invoke(canvasSpaceCorners);
            }
            else
            {
                Vector3[] p = new Vector3[canvasSpaceCorners.Length + 1];
                for (int i = 0; i < p.Length - 1; ++i)
                {
                    p[i] = canvasSpaceCorners[i];
                }
                p[p.Length - 1] = canvasSpaceCorners[0];
                Handles.DrawAAPolyLine(3, p);
            }
            if (Event.current.type == EventType.MouseDown)
            {
                for (int i = 0; i < cornersHandleRect.Length; ++i)
                {
                    if (cornersHandleRect[i].Contains(Event.current.mousePosition))
                    {
                        draggedPointsIndex = i;
                        break;
                    }
                    draggedPointsIndex = -1;
                }
            }
            else if (Event.current.type == EventType.MouseDrag)
            {
                if (draggedPointsIndex == -1)
                    return;
                int index = draggedPointsIndex;
                points[index] = FlipY(uvToCanvas.inverse.MultiplyPoint(Event.current.mousePosition));

                GUI.changed = true;
            }
            else if (Event.current.type == EventType.MouseUp)
            {
                draggedPointsIndex = -1;
            }
        }
    }
}

#endif
