#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Pinwheel.Vista.ExposeProperty
{
    public enum PropertyType
    {
        IntegerNumber = 0,
        RealNumber = 20,
        TrueFalse = 40,
        Text = 50,
        Vector = 60,
        Options = 70,
        Color = 80,
        Gradient = 90,
        Curve = 100,
        UnityObject = 110
    }
}
#endif
