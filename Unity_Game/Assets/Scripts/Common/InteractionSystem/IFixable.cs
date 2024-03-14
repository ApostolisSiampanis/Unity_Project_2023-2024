public interface IFixable
{
    bool CanBeFixed(Inventory.Inventory inventory);
    void Fix(Inventory.Inventory inventory);
}