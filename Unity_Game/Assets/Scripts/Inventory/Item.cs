using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item
{
    public enum ItemType
    {
        Apple,
        Carrot,
        Toolbox,
        Hammer,
        Book
    }

    public ItemType itemType;
    public int amount;

    public Sprite GetSprite()
    {
        switch (itemType)
        {
            default:
            case ItemType.Apple: return ItemAssets.Instance.appleSprite;
            case ItemType.Carrot: return ItemAssets.Instance.carrotsSprite;
            case ItemType.Toolbox: return ItemAssets.Instance.toolboxSprite;
            case ItemType.Hammer: return ItemAssets.Instance.hammerSprite;
            case ItemType.Book: return ItemAssets.Instance.bookSprite;
        }
    }

    public bool IsStackable()
    {
        switch (itemType) 
        {
            default:
            case ItemType.Apple:
            case ItemType.Carrot:
                return true;
            case ItemType.Hammer:
            case ItemType.Toolbox:
            case ItemType.Book:
                return false;
        }
    }
}