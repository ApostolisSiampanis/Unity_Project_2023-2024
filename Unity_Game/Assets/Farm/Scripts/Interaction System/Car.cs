using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class Car : Interactable
    {
        [SerializeField] private GameObject exitCutScene;
        [SerializeField] private GameObject gameCanvas;

        public override void OnInteract(Interactor interactor)
        {
            if (interactor == null) return;

            // Instant interaction
            Debug.Log("Interacted with car");
            gameCanvas.SetActive(false);
            exitCutScene.SetActive(true);
        }

        public override void OnEndInteract()
        {
            // TODO: Implement
        }

        public override bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
        {
            taskHint = this.taskHint;
            interactKey = this.interactKey;
            return readyToInteract;
        }

        public override void OnAbortInteract()
        {
            // TODO: Implement
        }
    }
}