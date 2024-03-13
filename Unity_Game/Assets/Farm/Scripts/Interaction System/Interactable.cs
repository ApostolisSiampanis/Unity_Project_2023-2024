using System;
using Farm.Scripts.Interaction_System;
using Farm.Scripts.QuestSystem;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

// Anything that we need to consider as Interactable needs to implement this class
public abstract class  Interactable : MonoBehaviour
{
    [Header("Interaction")] [SerializeField]
    protected string taskHint = "interact with something";
    public InteractQuest.InteractableObject interactableObject;

    [SerializeField]
    protected KeyCode interactKey;
    public bool readyToInteract = true;

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