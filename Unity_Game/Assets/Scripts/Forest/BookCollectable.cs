using Common.InteractionSystem;
using Inventory;

namespace Forest
{
    public class BookCollectable : Collectable
    {
        private void Start()
        {
            itemType = Item.ItemType.Book;
        }
    }
}