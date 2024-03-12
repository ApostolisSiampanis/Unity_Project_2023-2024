using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class Collectable : MonoBehaviour, Interactable
    {
        [Header("Interaction")] [SerializeField]
        private string _taskHint = "collect herb";

        [SerializeField] private KeyCode _interactKey;
        public bool readyToInteract = true;

        public Item.ItemType itemType;

        public virtual void OnInteract(Interactor interactor)
        {
            if (interactor == null) return;

            // Instant interaction
            interactor.Collect(itemType);
            interactor.EndInteraction(this);
        }

        public virtual void OnEndInteract()
        {
            // Remove object
            gameObject.SetActive(false);
        }

        public virtual bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
        {
            taskHint = _taskHint;
            interactKey = _interactKey;
            return readyToInteract;
        }

        public virtual void OnAbortInteract()
        {
            // TODO: Implement
        }
    }
}