#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista.Graph;

namespace Pinwheel.Vista.Graph
{
    public interface IExternalInputProvider
    {
        void SetInput(GraphInputContainer inputContainer);
        void CleanUp();
    } 
}
#endif
