using System.Collections.Generic;
using Farm.Scripts.Interaction_System;

namespace Farm.Scripts.QuestSystem
{
    public class InteractQuest : Quest
    {
        public enum InteractableItem
        {
            Sprinkler,
            Car
        }

        public List<Interactable> interactables;
        public InteractableItem interactableTarget;

        public override void StartQuest()
        {
            SetupQuest();
        }

        protected override void SetupQuest()
        {
            state = State.InProgress;
            interactables.ForEach(interactable => interactable.readyToInteract = true);
        }

        public override bool IsCompleted()
        {
            return state == State.Completed;
        }

        public override void CompleteQuest(Interactor interactor)
        {
            // TODO: Implement
        }

        protected override void CheckObjective()
        {
            if (currentAmount < requiredAmount) return;
            state = State.Completed;
        }

        public void ItemInteracted(InteractableItem interactableItem)
        {
            if (interactableTarget != interactableItem) return;
            currentAmount++;
            CheckObjective();
        }
    }
}