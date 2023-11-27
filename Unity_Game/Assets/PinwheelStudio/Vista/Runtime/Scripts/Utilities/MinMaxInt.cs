#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Vista
{
    [System.Serializable]
    public struct MinMaxInt
    {
        public static readonly MinMaxInt FULL_RANGE = new MinMaxInt(int.MinValue, int.MaxValue);

        [SerializeField]
        public int min;
        [SerializeField]
        public int max;

        public MinMaxInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public bool Equals(MinMaxInt other)
        {
            return this.min == other.min && this.max == other.max;
        }

        public static bool operator ==(MinMaxInt a, MinMaxInt b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(MinMaxInt a, MinMaxInt b)
        {
            return !a.Equals(b);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return (min, max).GetHashCode();
        }
    }
}
#endif
