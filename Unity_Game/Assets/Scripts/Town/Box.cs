using Common.InteractionSystem;
using Common.QuestSystem;

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
                if (QuestManager.instance.currentQuest is CarryQuest quest)
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