using UnityEngine;

namespace Farm.Scripts.InteractionSystem
{
    public class TreeCollectable : Collectable
    {
        public int minQuantity;
        public int maxQuantity;

        private int _availableFruits;

        // Start is called before the first frame update
        private void Start()
        {
            _availableFruits = Random.Range(minQuantity, maxQuantity);
            Debug.Log("Available fruits: " + _availableFruits);
        }

        public override void OnInteract(Interactor interactor)
        {
            if (interactor == null) return;

            // Instant interaction
            interactor.Collect(itemType);
            _availableFruits--;

            interactor.EndInteraction(this);
        }

        public override void OnEndInteract()
        {
            if (_availableFruits <= 0) readyToInteract = false;
        }
    }
}