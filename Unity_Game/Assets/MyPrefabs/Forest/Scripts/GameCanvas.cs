using UnityEngine;
using UnityEngine.Playables;

namespace MyPrefabs.Forest.Scripts
{
    public class GameCanvas : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector1;
        [SerializeField] private PlayableDirector playableDirector2;
        [SerializeField] private PlayableDirector playableDirector3;
        [SerializeField] private GameObject crosshair;
        [SerializeField] private GameObject miniMap;
        [SerializeField] private GameObject alex;

        private void Update()
        {
            // Disable crosshair and minimap when playable directors are enabled.
            bool isEnabled = playableDirector1.enabled || playableDirector2.enabled || playableDirector3.enabled;
            crosshair.SetActive(!isEnabled);
            miniMap.SetActive(!isEnabled);

            // Disable the farm-forest playable director when Alex is active.
            if (alex.activeInHierarchy) playableDirector1.enabled = false;
        }
    }
}