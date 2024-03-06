#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Geometric
{
    public struct Polygon2D
    {
        public Vector2[] vertices { get; set; }

        public Polygon2D(Vector2[] v)
        {
            if (v.Length < 3)
                throw new System.ArgumentException("A polygon must has at least 3 vertices");
            vertices = v;
        }

        public bool Contains(Vector2 p)
        {
            for (int i = 0; i < vertices.Length; ++i)
            {
                if (vertices[i] == p)
                {
                    return true;
                }
            }

            Vector2 lineStart = new Vector2(GetMinX() - 1, p.y);
            Vector2 lineEnd = p;
            Line2D hLine = new Line2D(lineStart, lineEnd);

            Line2D[] segments = GetSegments();
            Vector2 intersection;
            bool isIntersect;
            int intersectCount = 0;
            for (int i = 0; i < segments.Length; ++i)
            {
                isIntersect = Line2D.Intersect(hLine, segments[i], out intersection);
                intersectCount += isIntersect ? 1 : 0;
                if (isIntersect && intersection == p)
                {
                    return true;
                }
            }
            return intersectCount % 2 != 0;
        }

        private float GetMinX()
        {
            float minX = float.MaxValue;
            foreach (Vector2 v in vertices)
            {
                minX = Mathf.Min(minX, v.x);
            }
            return minX;
        }

        public Line2D[] GetSegments()
        {
            Line2D[] segments = new Line2D[vertices.Length];
            segments[0] = new Line2D(vertices[0], vertices[vertices.Length - 1]);
            for (int i = 1; i < vertices.Length; ++i)
            {
                segments[i] = new Line2D(vertices[i], vertices[i - 1]);
            }
            return segments;
        }

        public static bool IsOverlap(Polygon2D polygon0, Polygon2D polygon1)
        {
            foreach (Vector2 v in polygon0.vertices)
            {
                if (polygon1.Contains(v))
                    return true;
            }

            foreach (Vector2 v in polygon1.vertices)
            {
                if (polygon0.Contains(v))
                    return true;
            }

            Line2D[] segments0 = polygon0.GetSegments();
            Line2D[] segments1 = polygon1.GetSegments();
            Vector2 intersection;
            for (int i = 0; i < segments0.Length; ++i)
            {
                for (int j = 0; j < segments1.Length; ++j)
                {
                    if (Line2D.Intersect(segments0[i], segments1[j], out intersection))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
#endif
