using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using UnityEngine;

public class AppleCollectQuest : CollectQuest
{
    public override void StartQuest()
    {
        state = State.InProgress;
        collectables.ForEach(collectable => collectable.readyToInteract = true);
    }

    protected override void SetupQuest()
    {
        throw new System.NotImplementedException();
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

    public override void ItemCollected(Item item)
    {
        if (itemType != item.itemType) return;
        currentAmount += item.amount;
        CheckObjective();

    }
}
