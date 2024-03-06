#if VISTA
using System;
using UnityEngine;

namespace Pinwheel.Vista.Geometric
{
    public struct Line2D : IEquatable<Line2D>
    {
        public Vector2 startPoint { get; set; }
        public Vector2 endPoint { get; set; }

        public Vector2 Direction
        {
            get
            {
                return (endPoint - startPoint).normalized;
            }
        }

        public float Length
        {
            get
            {
                return (startPoint - endPoint).magnitude;
            }
        }

        public float SqrLength
        {
            get
            {
                return (startPoint - endPoint).sqrMagnitude;
            }
        }

        public Line2D(Vector2 start, Vector2 end)
        {
            startPoint = start;
            endPoint = end;
        }

        public Line2D(float x1, float y1, float x2, float y2)
        {
            startPoint = new Vector2(x1, y1);
            endPoint = new Vector2(x2, y2);
        }

        public float GetX(float y)
        {
            Vector2 dir = endPoint - startPoint;
            float a = -dir.y;
            float b = dir.x;
            float c = -(a * startPoint.x + b * startPoint.y);
            float x = (-b * y - c) / a;
            return x;
        }

        public float GetY(float x)
        {
            Vector2 dir = endPoint - startPoint;
            float a = -dir.y;
            float b = dir.x;
            float c = -(a * startPoint.x + b * startPoint.y);
            float y = (-a * x - c) / b;
            return y;
        }

        public static bool Intersect(Line2D l1, Line2D l2, out Vector2 point)
        {
            bool result = false;
            float x1 = l1.startPoint.x;
            float x2 = l1.endPoint.x;
            float x3 = l2.startPoint.x;
            float x4 = l2.endPoint.x;
            float y1 = l1.startPoint.y;
            float y2 = l1.endPoint.y;
            float y3 = l2.startPoint.y;
            float y4 = l2.endPoint.y;

            float denominator = (x1 - x2) * (y3 - y4) - (y1 - y2) * (x3 - x4);
            if (denominator == 0)
            {
                point = new Vector2(0, 0);
                result = false;
            }
            else
            {
                float xNumerator = (x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4);
                float yNumerator = (x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4);
                point = new Vector2(xNumerator / denominator, yNumerator / denominator);
                float sqrLength1 = l1.SqrLength;
                float sqrLength2 = l2.SqrLength;
                if ((point - l1.startPoint).sqrMagnitude > sqrLength1 || (point - l1.endPoint).sqrMagnitude > sqrLength1)
                {
                    result = false;
                }
                else if ((point - l2.startPoint).sqrMagnitude > sqrLength2 || (point - l2.endPoint).sqrMagnitude > sqrLength2)
                {
                    result = false;
                }
                else
                {
                    result = true;
                }
            }

            return result;
        }

        public bool Equals(Line2D other)
        {
            return this.startPoint == other.startPoint && this.endPoint == other.endPoint;
        }
    }
}
#endif
