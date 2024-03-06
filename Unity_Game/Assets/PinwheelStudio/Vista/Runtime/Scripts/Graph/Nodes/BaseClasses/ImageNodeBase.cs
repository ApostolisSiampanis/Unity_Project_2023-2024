#if VISTA
using UnityEngine;

namespace Pinwheel.Vista.Graph
{
    [System.Serializable]
    public abstract class ImageNodeBase : ExecutableNodeBase
    {
        [SerializeField]
        protected ResolutionOverrideOptions m_resolutionOverride;
        public ResolutionOverrideOptions resolutionOverride
        {
            get
            {
                return m_resolutionOverride;
            }
            set
            {
                m_resolutionOverride = value;
            }
        }

        [SerializeField]
        protected float m_resolutionMultiplier;
        public float resolutionMultiplier
        {
            get
            {
                return m_resolutionMultiplier;
            }
            set
            {
                m_resolutionMultiplier = Mathf.Clamp(value, 0.1f, 2f);
            }
        }

        [SerializeField]
        protected int m_resolutionAbsolute;
        public int resolutionAbsolute
        {
            get
            {
                return m_resolutionAbsolute;
            }
            set
            {
                m_resolutionAbsolute = Utilities.MultipleOf8(Mathf.Clamp(value, Constants.RES_MIN, Constants.RES_MAX));
            }
        }

        public ImageNodeBase() : base()
        {
            this.m_resolutionOverride = ResolutionOverrideOptions.RelativeToMainInput;
            this.m_resolutionMultiplier = 1;
            this.m_resolutionAbsolute = 1024;
        }
    }
}
#endif
