#if VISTA
using UnityEngine;

namespace Pinwheel.Vista
{
    public class ProgressiveTask : CustomYieldInstruction
    {
        public bool isCompleted { get; private set; }
        public override bool keepWaiting
        {
            get
            {
                return !isCompleted;
            }
        }        

        public void Complete()
        {
            isCompleted = true;
        }
    }
}
#endif
