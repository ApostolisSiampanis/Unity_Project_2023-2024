using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.DialogueSystem;
using Farm.Scripts.Interaction_System;
using Farm.Scripts.InteractionSystem;
using Farm.Scripts.QuestSystem;
using UnityEngine;

public class Sprinkler : Interactable, IFixable
{
    public bool isBroken = true;
    public Item.ItemType requiredItemToFix;
    
    private DialogueManager _dialogueManager;

    public void Start()
    {
        _dialogueManager = FindObjectOfType<DialogueManager>();
        
        if (_dialogueManager == null) Debug.LogError("Dialogue Manager is missing");
    }

    public override void OnInteract(Interactor interactor)
    {
        base.OnInteract(interactor);
        
        if (isBroken) Debug.Log("I think it's broken. I have to tell grandpa!");
        
        // Instant interaction
        interactor.EndInteraction(this);
    }

    public override void OnEndInteract()
    {
        // TODO: Implement
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
