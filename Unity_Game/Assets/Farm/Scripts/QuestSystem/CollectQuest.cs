using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using UnityEngine;
using UnityEngine.Serialization;

public class CollectQuest : Quest
{
    public List<Collectable> collectables;
    public Item.ItemType itemType;
    
    public override void StartQuest()
    {
        SetupQuest();
    }

    protected override void SetupQuest()
    {
        state = State.InProgress;
        collectables.ForEach(collectable => collectable.readyToInteract = true);
    }

    public override bool IsCompleted()
    {
        return state == State.Completed;
    }

    public override void CompleteQuest(Interactor interactor)
    {
        interactor.inventory.RemoveAllItems(itemType);
    }

    protected override void CheckObjective()
    {
        if (currentAmount < requiredAmount) return;
        state = State.Completed;
        collectables.ForEach(collectable => collectable.readyToInteract = false);
    }
    
    public void ItemCollected(Item item)
    {
        if (itemType != item.itemType) return;
        currentAmount += item.amount;
        CheckObjective();
    }
}
