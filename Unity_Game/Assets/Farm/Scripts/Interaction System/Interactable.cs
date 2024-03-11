using JetBrains.Annotations;
using UnityEngine;

// Anything that we need to consider as Interactable needs to implement this interface
namespace Farm.Scripts.Interaction_System
{
    public interface Interactable
    {
        // Called when the Interactor wants to interact with the selected object
        void OnInteract(Interactor interactor);
    
        // Called when interaction has started and now ends (Forced or not).
        void OnEndInteract();
    
        // Called when object gets selected and ready to interact
        bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey);
    
        // Called when object gets deselected from interacting
        void OnAbortInteract();
    }
}
