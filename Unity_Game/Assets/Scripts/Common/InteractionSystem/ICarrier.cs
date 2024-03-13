using Farm.Scripts.InteractionSystem;

namespace Common.InteractionSystem
{
    public interface ICarrier
    {
        void PickUp(Interactable.InteractableObject interactableObject);
        void Drop();
        Interactable.InteractableObject GetCarryingObject();
    }
}
