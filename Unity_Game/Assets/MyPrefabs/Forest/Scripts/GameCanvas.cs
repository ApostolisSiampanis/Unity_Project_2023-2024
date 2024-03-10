using UnityEngine;
using UnityEngine.Playables;

namespace MyPrefabs.Forest.Scripts
{
    public class GameCanvas : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector1;
        [SerializeField] private PlayableDirector playableDirector2;
        [SerializeField] private GameObject crosshair;
        [SerializeField] private GameObject miniMap;

        private void Update()
        {
            bool isEnabled = playableDirector1.enabled || playableDirector2.enabled;
            crosshair.SetActive(!isEnabled);
            miniMap.SetActive(!isEnabled);
        }
    }
}