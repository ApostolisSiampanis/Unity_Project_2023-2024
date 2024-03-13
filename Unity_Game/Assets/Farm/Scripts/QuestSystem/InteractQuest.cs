using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using Farm.Scripts.InteractionSystem;
using UnityEngine;

namespace Farm.Scripts.QuestSystem
{
    public class InteractQuest : Quest
    {
        
        public List<Interactable> interactables;
        public Interactable.InteractableObject interactableTarget;
        
        [Header("Flags")]
        public bool setInteractableAfterQuest;

        protected override void SetupQuest()
        {
            state = State.InProgress;
            interactables.ForEach(interactable => interactable.readyToInteract = true);
        }

        public override void CompleteQuest(Interactor interactor, NPC requester)
        {
            QuestManager.Instance.CompleteQuest();
        }

        protected override void CheckObjective()
        {
            if (currentAmount < requiredAmount) return;
            state = State.Completed;
            interactables.ForEach(interactable => interactable.readyToInteract = setInteractableAfterQuest);
            if (responsibleNPC == null) CompleteQuest(null, null);
        }

        public void ObjectInteracted(Interactable.InteractableObject interactableObject)
        {
            if (interactableTarget != interactableObject) return;
            currentAmount++;
            CheckObjective();
        }
    }
}