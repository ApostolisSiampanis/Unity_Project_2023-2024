using System.Collections.Generic;
using Common.InteractionSystem;
using UnityEngine;

namespace Save.Data
{
    [System.Serializable]
    public class TownData
    {
        public int questIndex { get; set; }
        public float[] playerPosition { get; set; }

        public TownData(int questIndex, Vector3 playerPosition)
        {
            this.questIndex = questIndex;
            this.playerPosition = new[] { playerPosition.x, playerPosition.y, playerPosition.z };
        }
    }
}
