using Farm.Scripts.Interaction_System;
using Farm.Scripts.QuestSystem;
using JetBrains.Annotations;
using UnityEngine;

// Anything that we need to consider as Interactable needs to implement this class
namespace Farm.Scripts.InteractionSystem
{
    public abstract class Interactable : MonoBehaviour
    {
        [Header("Interaction")] [SerializeField]
        public string taskHint = "interact with something";
        public InteractableObject interactableObject;

        [SerializeField]
        protected KeyCode interactKey;
        public bool readyToInteract = true;
    
        public enum InteractableObject
        {
            None,
            Sprinkler,
            Car,
            Carrot,
            AppleTree,
            Box,
            Grandpa,
            Alice,
            Samir,
            Bob
        }

        // Called when the Interactor wants to interact with the selected object
        public virtual void OnInteract(Interactor interactor)
        {
            if (interactor == null)
            {
                Debug.LogError("Interactor is null");
            }
        
            if (QuestManager.Instance.currentQuest != null && QuestManager.Instance.currentQuest is InteractQuest quest)
            {
                quest.ObjectInteracted(interactableObject);
            }
        }

        // Called when interaction has started and now ends (Forced or not).
        public abstract void OnEndInteract();

        // Called when object gets selected and ready to interact
        public virtual bool IsReadyToInteract([CanBeNull] out string taskHint, out KeyCode interactKey)
        {
            taskHint = this.taskHint;
            interactKey = this.interactKey;
            return readyToInteract;
        }

        // Called when object gets deselected from interacting
        public abstract void OnAbortInteract();
    }
}