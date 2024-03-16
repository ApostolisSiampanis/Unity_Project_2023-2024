using UnityEngine;

namespace Save.Data
{
    [System.Serializable]
    public class MainProgressData
    {
        public int sceneIndex { get; set; }

        public MainProgressData(int sceneIndex)
        {
            this.sceneIndex = sceneIndex;
        }
    }
}
