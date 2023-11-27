#if VISTA

namespace Pinwheel.Vista.Graph
{
    public class ExecutionProgress
    {
        public float totalProgress { get; internal set; }
        public float currentProgress { get; set; }

        public ExecutionProgress()
        {
            totalProgress = 0f;
            currentProgress = 0f;
        }
    }
}
#endif
