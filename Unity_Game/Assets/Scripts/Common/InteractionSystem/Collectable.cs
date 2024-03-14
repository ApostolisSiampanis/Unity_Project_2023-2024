using Inventory;
using UnityEngine;

namespace Common.InteractionSystem
{
    public class Collectable : Interactable
    {
        public Item.ItemType itemType;

        public override void OnInteract(Interactor interactor)
        {
            if (interactor == null) return;

            // Instant interaction
            interactor.Collect(itemType);
            interactor.EndInteraction(this);
        }

        public override void OnEndInteract()
        {
            // Remove object
            gameObject.SetActive(false);
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