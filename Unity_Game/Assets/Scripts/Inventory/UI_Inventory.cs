using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Inventory
{
    public class UI_Inventory : MonoBehaviour
    {
        private Inventory _inventory;
        private Transform _itemSlotContainer;
        private Transform _itemSlotTemplate;

        private void Awake()
        {
            _itemSlotContainer = transform.Find("itemSlotContainer");
            _itemSlotTemplate = _itemSlotContainer.Find("itemSlotTemplate");
        }

        public void SetInventory(Inventory inventory)
        {
            this._inventory = inventory;
            inventory.onItemListChanged += Inventory_OnItemListChanged;
            RefreshInventoryItems();
        }

        private void Inventory_OnItemListChanged(object sender, EventArgs e)
        {
            RefreshInventoryItems();
        }

        private void RefreshInventoryItems()
        {
            foreach (Transform child in _itemSlotContainer)
            {
                if (child == _itemSlotTemplate) continue;
                Destroy(child.gameObject);
            }

            int x = 0;
            int y = 0;
            const float itemSlotCellSize = 60f;
            foreach (Item item in _inventory.GetItemList())
            {
                RectTransform itemSlotRectTransform =
                    Instantiate(_itemSlotTemplate, _itemSlotContainer).GetComponent<RectTransform>();
                itemSlotRectTransform.gameObject.SetActive(true);

                // We have only one item to set on a slot
                itemSlotRectTransform.anchoredPosition = new Vector2(x * itemSlotCellSize, y * itemSlotCellSize);
                Image image = itemSlotRectTransform.Find("image").GetComponent<Image>();
                image.sprite = item.GetSprite();

                TextMeshProUGUI uiText = itemSlotRectTransform.Find("amountText").GetComponent<TextMeshProUGUI>();
                if (item.amount > 1)
                {
                    uiText.SetText(item.amount.ToString());
                }
                else
                {
                    uiText.SetText("");
                }
            }
        }
    }
}