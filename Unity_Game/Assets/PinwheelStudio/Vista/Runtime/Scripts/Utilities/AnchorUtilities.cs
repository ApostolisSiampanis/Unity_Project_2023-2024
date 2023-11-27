#if VISTA
using System.Collections.Generic;
using UnityEngine;

namespace Pinwheel.Vista
{
    public static class AnchorUtilities
    {
        public static Vector3[] Insert(Vector3[] srcAnchors, Vector3 newAnchor)
        {
            List<Vector3> anchors = new List<Vector3>(srcAnchors);
            if (anchors.Count < 2)
            {
                anchors.Add(newAnchor);
            }
            else
            {
                int insertIndex = GetInsertIndex(anchors, newAnchor);
                anchors.Insert(insertIndex, newAnchor);
            }
            return anchors.ToArray();
        }

        public static int GetInsertIndex(List<Vector3> anchors, Vector3 newAnchor)
        {
            int insertIndex = -1;
            if (anchors.Count < 2)
            {
                return insertIndex;
            }

            float d = 0;
            float minDistance = float.MaxValue;
            Vector3 center;

            center = (anchors[0] + anchors[anchors.Count - 1]) * 0.5f;
            d = Vector3.Distance(newAnchor, center);
            if (d < minDistance)
            {
                minDistance = d;
                insertIndex = 0;
            }
            for (int i = 1; i < anchors.Count; ++i)
            {
                center = (anchors[i] + anchors[i - 1]) * 0.5f;
                d = Vector3.Distance(newAnchor, center);
                if (d < minDistance)
                {
                    minDistance = d;
                    insertIndex = i;
                }
            }
            return insertIndex;
        }

        public static Vector3[] Remove(Vector3[] srcAnchors, Vector3 anchorToRemove)
        {
            List<Vector3> anchors = new List<Vector3>(srcAnchors);
            anchors.Remove(anchorToRemove);
            return anchors.ToArray();
        }

        public static Vector3[] RemoveAt(Vector3[] srcAnchors, int i)
        {
            List<Vector3> anchors = new List<Vector3>(srcAnchors);
            anchors.RemoveAt(i);
            return anchors.ToArray();
        }

        public static void Transform(Vector3[] srcAnchor, Matrix4x4 matrix)
        {
            for (int i = 0; i < srcAnchor.Length; ++i)
            {
                srcAnchor[i] = matrix.MultiplyPoint(srcAnchor[i]);
            }
        }

        public static void FlattenY(Vector3[] srcAnchor, float y)
        {
            for (int i = 0; i < srcAnchor.Length; ++i)
            {
                srcAnchor[i].y = y;
            }
        }

        public static Vector3[] GetFalloff(Vector3[] srcAnchors, float distance, FalloffDirection direction)
        {
            if (srcAnchors.Length < 3)
            {
                Vector3[] falloff = new Vector3[srcAnchors.Length];
                srcAnchors.CopyTo(falloff, 0);
                return falloff;
            }
            else
            {
                Vector3[] falloff = new Vector3[srcAnchors.Length];
                bool reverse = false;
                for (int i = 0; i < falloff.Length; ++i)
                {
                    Vector3[] segments = GetAdjacentSegments(srcAnchors, i);
                    Vector2 dir0 = (segments[1].XZ() - segments[0].XZ()).normalized;
                    Vector2 dir1 = (segments[2].XZ() - segments[1].XZ()).normalized;
                    Vector2 dir = (dir0 + dir1).normalized;
                    if (i == 0)
                    {
                        Vector3 cross = Vector3.Cross(dir0, dir1);
                        reverse = cross.z >= 0;
                    }
                    Vector3 normal;
                    int mul = direction == FalloffDirection.Outer ? 1 : -1;
                    if (reverse)
                    {
                        normal = new Vector3(dir.y, 0, -dir.x) * mul;
                    }
                    else
                    {
                        normal = new Vector3(-dir.y, 0, dir.x) * mul;
                    }
                    falloff[i] = srcAnchors[i] + normal * distance;
                }
                return falloff;
            }
        }

        private static Vector3[] GetAdjacentSegments(Vector3[] srcAnchors, int index)
        {
            if (srcAnchors.Length < 3)
                throw new System.Exception("Can't get segments, vertex count must >= 3");
            if (index < 0 || index >= srcAnchors.Length)
                throw new System.Exception("Invalid vertex index");
            if (index == 0)
            {
                return new Vector3[3]
                {
                    srcAnchors[srcAnchors.Length - 1],
                    srcAnchors[0],
                    srcAnchors[1]
                };
            }
            else if (index == srcAnchors.Length - 1)
            {
                return new Vector3[3]
                {
                    srcAnchors[srcAnchors.Length - 2],
                    srcAnchors[srcAnchors.Length - 1],
                    srcAnchors[0]
                };
            }
            else
            {
                return new Vector3[3]
                {
                    srcAnchors[index - 1],
                    srcAnchors[index],
                    srcAnchors[index + 1]
                };
            }
        }

        public static void SnapToWorld(Vector3[] srcAnchor)
        {
            for (int i = 0; i < srcAnchor.Length; ++i)
            {
                Vector3 p = srcAnchor[i];
                Vector3 rayOrigin = p;
                rayOrigin.y = 10000;
                Ray r = new Ray(rayOrigin, Vector3.down);
                RaycastHit hit;
                if (Physics.Raycast(r, out hit, 20000))
                {
                    p.y = hit.point.y;
                }
                srcAnchor[i] = p;
            }
        }

        public static Vector3 SnapToWorld(Vector3 srcAnchor)
        {
            Vector3 rayOrigin = srcAnchor;
            rayOrigin.y = 10000;
            Ray r = new Ray(rayOrigin, Vector3.down);
            RaycastHit hit;
            if (Physics.Raycast(r, out hit, 20000))
            {
                srcAnchor.y = hit.point.y;
            }
            return srcAnchor;
        }
    }
}
#endif
