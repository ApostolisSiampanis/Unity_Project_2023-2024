using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.DialogueSystem;
using Farm.Scripts.Interaction_System;
using UnityEngine;

public class Sprinkler : Interactable, IFixable
{
    public bool isBroken = true;
    public Item.ItemType requiredItemToFix;

    private QuestManager _questManager;
    private DialogueManager _dialogueManager;

    public void Start()
    {
        _questManager = QuestManager.Instance;
        _dialogueManager = FindObjectOfType<DialogueManager>();
        
        if (_questManager == null) Debug.LogError("Quest Manager is missing");
        if (_dialogueManager == null) Debug.LogError("Dialogue Manager is missing");
    }

    public override void OnInteract(Interactor interactor)
    {
        if (interactor == null) return;

        // Instant interaction
        if (isBroken) Debug.Log("I think it's broken. I have to tell grandpa!");

        if (_questManager.currentQuest != null && _questManager.currentQuest is InteractQuest quest)
        {
            quest.ItemInteracted(InteractQuest.InteractableItem.Sprinkler);
        }
        interactor.EndInteraction(this);
    }

    public override void OnEndInteract()
    {
        // TODO: Implement
    }

    public override bool IsReadyToInteract(out string taskHint, out KeyCode interactKey)
    {
        taskHint = this.taskHint;
        interactKey = this.interactKey;
        return readyToInteract;
    }

    public override void OnAbortInteract()
    {
        // TODO: Implement
    }
    
    public bool CanBeFixed(Inventory inventory)
    {
        return inventory.GetItemList().Find(item => item.itemType == requiredItemToFix) != null;
        
    }

    public void Fix(Inventory inventory)
    {
        if (!CanBeFixed(inventory)) return;
        // Remove the used item?
        isBroken = false;
    }
}