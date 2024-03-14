using UnityEngine;

namespace Inventory
{
    [System.Serializable]
    public class Item
    {
        public enum ItemType
        {
            Apple,
            Carrot,
            Toolbox,
            Hammer,
            Book,
            Box,
            None
        }

        public ItemType itemType;
        public int amount;

        public Sprite GetSprite()
        {
            switch (itemType)
            {
                default:
                case ItemType.Apple: return ItemAssets.instance.appleSprite;
                case ItemType.Carrot: return ItemAssets.instance.carrotsSprite;
                case ItemType.Toolbox: return ItemAssets.instance.toolboxSprite;
                case ItemType.Hammer: return ItemAssets.instance.hammerSprite;
                case ItemType.Book: return ItemAssets.instance.bookSprite;
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
}