using Inventory;

namespace Common.InteractionSystem
{
    public interface ICollector
    {
        void Collect(Item.ItemType itemType);
    }
}