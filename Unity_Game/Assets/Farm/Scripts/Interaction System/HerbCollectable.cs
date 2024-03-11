using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class HerbCollectable : MonoBehaviour, Interactable
    {
        [Header("Interaction")]
        [SerializeField] private string _taskHint = "collect herb";
        [SerializeField] private KeyCode _interactKey;
        [SerializeField] private bool _readyToInteract = true;

        public void OnInteract(Interactor interactor)
        {
            // Instant interaction
            interactor.EndInteraction(this);
        }

        public void OnEndInteract()
        {
            // Remove object
            gameObject.SetActive(false);
        }

        public bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
        {
            taskHint = _taskHint;
            interactKey = _interactKey;
            return _readyToInteract;
        }

        public void OnAbortInteract()
        {
            // TODO: Implement
        }
    }
}
