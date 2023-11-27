#if VISTA
using System;
using System.Collections.Generic;

namespace Pinwheel.Vista.Graph
{
    public class ExecutionHandle : ProgressiveTask, IDisposable
    {
        private List<CoroutineHandle> m_coroutines;
        internal List<CoroutineHandle> coroutines
        {
            get
            {
                return m_coroutines;
            }
        }

        private DataPool m_data;
        public DataPool data
        {
            get
            {
                return m_data;
            }
        }

        private ExecutionProgress m_progress;
        public ExecutionProgress progress
        {
            get
            {
                return m_progress;
            }
        }

        public static ExecutionHandle Create()
        {
            ExecutionHandle handle = new ExecutionHandle();
            handle.m_data = new DataPool();
            handle.m_progress = new ExecutionProgress();
            handle.m_coroutines = new List<CoroutineHandle>();
            return handle;
        }

        public void Dispose()
        {
            if (m_coroutines != null)
            {
                foreach (CoroutineHandle c in m_coroutines)
                {
                    CoroutineUtility.StopCoroutine(c);
                }
            }
            if (m_data != null)
            {
                m_data.Dispose();
            }
        }
    }
}
#endif
