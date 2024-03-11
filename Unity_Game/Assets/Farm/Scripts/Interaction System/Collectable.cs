using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class Collectable : MonoBehaviour, Interactable
    {
        [Header("Interaction")]
        [SerializeField] private string _taskHint = "collect herb";
        [SerializeField] private KeyCode _interactKey;
        [SerializeField] private bool _readyToInteract = true;

        public Item.ItemType itemType;

        public void OnInteract(Interactor interactor)
        {
            if (interactor == null) return;
            
            // Instant interaction
            interactor.Collect(itemType);
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
