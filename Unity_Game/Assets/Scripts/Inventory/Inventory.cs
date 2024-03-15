using System;
using System.Collections.Generic;
using Common.QuestSystem;
using UnityEngine;

namespace Inventory
{
    public class Inventory
    {
        public event EventHandler onItemListChanged;

        private readonly List<Item> _itemList;

        public Inventory()
        {
            _itemList = new List<Item>();
        }

        public void AddItem(Item item)
        {
            if (item.IsStackable())
            {
                bool itemAlreadyInInventory = false;
                foreach (Item inventoryItem in _itemList)
                {
                    if (inventoryItem.itemType == item.itemType)
                    {
                        inventoryItem.amount += item.amount;
                        itemAlreadyInInventory = true;
                    }
                }

                if (!itemAlreadyInInventory)
                {
                    _itemList.Add(item);
                }
            }
            else
            {
                _itemList.Add(item);
            }

            if (QuestManager.instance.currentQuest is CollectQuest quest)
            {
                quest.ItemCollected(item);
            }

            onItemListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveAllItems(Item.ItemType type)
        {
            _itemList.RemoveAll(x => x.itemType == type);
            onItemListChanged?.Invoke(this, EventArgs.Empty);
        }

        public List<Item> GetItemList()
        {
            return _itemList;
        }
    }
}