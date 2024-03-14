using Common.DialogueSystem;
using Common.InteractionSystem;
using Inventory;
using UnityEngine;

namespace Farm.InteractionSystem
{
    public class Sprinkler : Interactable, IFixable
    {
        public bool isBroken = true;
        public Item.ItemType requiredItemToFix;
        public Dialogue onBrokenMonologue;

        private DialogueManager _dialogueManager;

        public void Start()
        {
            _dialogueManager = FindObjectOfType<DialogueManager>();

            if (_dialogueManager == null) Debug.LogError("Dialogue Manager is missing");
        }

        public override void OnInteract(Interactor interactor)
        {
            base.OnInteract(interactor);

            if (isBroken)
            {
                // Trigger monologue
                interactor.TriggerMonologue(onBrokenMonologue);
            }
            else
            {
                // Instant interaction
                interactor.EndInteraction(this);
            }
            
        }

        public override void OnEndInteract()
        {
            // TODO: Implement
        }

        public override void OnAbortInteract()
        {
            // TODO: Implement
        }

        public bool CanBeFixed(Inventory.Inventory inventory)
        {
            return inventory.GetItemList().Find(item => item.itemType == requiredItemToFix) != null;
        }

        public void Fix(Inventory.Inventory inventory)
        {
            if (!CanBeFixed(inventory)) return;
            // Remove the used item?
            isBroken = false;
        }
    }
}