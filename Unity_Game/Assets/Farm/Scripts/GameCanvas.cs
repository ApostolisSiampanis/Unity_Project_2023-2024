using UnityEngine;
using UnityEngine.Playables;

namespace Farm.Scripts
{
    public class GameCanvas : MonoBehaviour
    {
        [SerializeField] private PlayableDirector playableDirector1;
        [SerializeField] private PlayableDirector playableDirector2;
        [SerializeField] private GameObject crosshair;
        [SerializeField] private GameObject miniMap;
        [SerializeField] private GameObject alex;
        [SerializeField] private Wheels wheels;

        private void Update()
        {
            // // Disable crosshair and minimap when playable directors are enabled.
            // bool isEnabled = playableDirector1.enabled || playableDirector2.enabled;
            // crosshair.SetActive(!isEnabled);
            // miniMap.SetActive(!isEnabled);
            //
            // // If Alex is not active, return.
            // if (!alex.activeSelf) return;
            //
            // // Alex is active.
            // // Disable the farm-forest playable director.
            // playableDirector1.enabled = false;
            //
            // // Disable the wheels script when Alex is active. Means that the car is not moving.
            // wheels.enabled = false;
        }
    }
}