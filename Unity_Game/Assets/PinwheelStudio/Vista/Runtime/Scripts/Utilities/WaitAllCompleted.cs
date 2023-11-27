#if VISTA
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Pinwheel.Vista;
using Pinwheel.Vista.Graphics;

namespace Pinwheel.Vista
{
    public class WaitAllCompleted : CustomYieldInstruction
    {
        private ProgressiveTask[] m_tasks;

        public WaitAllCompleted(params ProgressiveTask[] tasks)
        {
            m_tasks = tasks;
        }

        public override bool keepWaiting
        {
            get
            {
                foreach (ProgressiveTask t in m_tasks)
                {
                    if (!t.isCompleted)
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
#endif
