using UnityEngine;

namespace Farm.Scripts.Interaction_System
{
    public class TreeCollectable : Collectable
    {
        public int minQuantity;
        public int maxQuantity;

        private int _availableFruits;
    
        // Start is called before the first frame update
        void Start()
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
            
            if (_availableFruits <= 0) readyToInteract = false;
            
            interactor.EndInteraction(this);
            Debug.Log("Collected 1 fruit from tree");
        }
        
        public override void OnEndInteract()
        {
            // Do nothing
            Debug.Log("On end interact with tree");
        }
    }
}
