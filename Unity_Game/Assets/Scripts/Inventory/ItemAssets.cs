using UnityEngine;

namespace Inventory
{
    public class ItemAssets : MonoBehaviour
    {
        public static ItemAssets instance { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        public Sprite appleSprite;
        public Sprite carrotsSprite;
        public Sprite toolboxSprite;
        public Sprite hammerSprite;
        public Sprite bookSprite;
    }
}