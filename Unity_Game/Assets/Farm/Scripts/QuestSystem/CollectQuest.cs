using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class CollectQuest : Quest
{
    public List<Collectable> collectables;
    public Item.ItemType itemType;

    public abstract void ItemCollected(Item item);
}
