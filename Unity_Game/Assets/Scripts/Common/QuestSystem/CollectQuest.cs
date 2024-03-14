using System.Collections.Generic;
using Common.InteractionSystem;
using Common.QuestSystem;
using Inventory;
using UnityEngine;

public class CollectQuest : Quest
{
    public List<Collectable> collectables;
    public Item.ItemType itemType;
    public Item Reward;

    protected override void SetupQuest()
    {
        state = State.InProgress;
        collectables.ForEach(collectable => collectable.readyToInteract = true);
    }

    public override void CompleteQuest(Interactor interactor, NPC requester)
    {
        if (responsibleNPC != null && (requester != responsibleNPC || state != State.Completed))
        {
            Debug.LogError("Quest cannot be completed");
            return;
        }

        interactor.inventory.RemoveAllItems(itemType);
        if (Reward.itemType != Item.ItemType.None) interactor.inventory.AddItem(Reward);
        QuestManager.Instance.CompleteQuest();
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