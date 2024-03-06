#if VISTA
using System;

namespace Pinwheel.Vista
{
    public static class Constants
    {
        public static int K1024
        {
            get
            {
                return 1024;
            }
        }

        public static int K1025
        {
            get
            {
                return 1025;
            }
        }

        public static int RES_MIN
        {
            get
            {
                return 32;
            }
        }

        internal static event Func<int> getResMaxCallback;
        public static int RES_MAX
        {
            get
            {
                if (getResMaxCallback != null)
                {
                    return getResMaxCallback.Invoke();
                }
                else
                {
                    return K1025;
                }
            }
        }

        public static int HM_RES_MIN
        {
            get
            {
                return 33;
            }
        }

        internal static event Func<int> getHmResMaxCallback;
        public static int HM_RES_MAX
        {
            get
            {
                if (getHmResMaxCallback != null)
                {
                    return getHmResMaxCallback.Invoke();
                }
                else
                {
                    return K1025;
                }
            }
        }


    }
}
#endif
