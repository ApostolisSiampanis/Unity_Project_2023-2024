using Common;
using UnityEngine;

namespace Timelines
{
    public class SceneLoader : MonoBehaviour
    {
        public LevelLoader.Scene nextScene;
        public LevelLoader levelLoader;
        
        void Start()
        {
            levelLoader.LoadScene((int)nextScene);
        }

    }
}
