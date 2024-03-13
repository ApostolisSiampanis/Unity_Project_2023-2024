using System;
using Common.QuestSystem;
using Farm.Scripts.Interaction_System;
using Farm.Scripts.InteractionSystem;
using UnityEngine;

namespace Town
{
    public class Box : Interactable
    {
        public void Start()
        {
            interactableObject = InteractableObject.Box;
        }

        public override void OnInteract(Interactor interactor)
        {
            base.OnInteract(interactor);
            
            // Instant interaction
            if (interactor.GetCarryingObject() == InteractableObject.None)
            {
                interactor.PickUp(interactableObject);
                if (QuestManager.Instance.currentQuest is CarryQuest quest)
                {
                    quest.OnObjectPickUp(interactableObject);
                }
                gameObject.SetActive(false);
            }
            
            interactor.EndInteraction(this);
        }

        public override void OnEndInteract()
        {
            // Do Nothing
        }

        public override void OnAbortInteract()
        {
            // Do Nothing
        }
    }
}
