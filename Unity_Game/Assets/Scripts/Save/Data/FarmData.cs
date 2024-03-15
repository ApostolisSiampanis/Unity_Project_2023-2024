using System.Collections.Generic;
using Common.InteractionSystem;
using UnityEngine;

namespace Save.Data
{
    [System.Serializable]
    public class FarmData
    {
        public int questIndex { get; set; }
        public float[] playerPosition { get; set; }
        public bool[] carrotsVisibility { get; set; }

        public FarmData(int questIndex, Vector3 playerPositionVector, List<Collectable> carrots)
        {
            this.questIndex = questIndex;
            playerPosition = new[] { playerPositionVector.x, playerPositionVector.y, playerPositionVector.z };
            carrotsVisibility = CreateVisibilityArray(carrots);
        }

        private bool[] CreateVisibilityArray(List<Collectable> collectables)
        {
            if (collectables == null) return null;
            var temp = new bool[collectables.Count];
            for (var i = 0; i < collectables.Count; i++)
            {
                temp[i] = collectables[i].gameObject.activeSelf;
            }

            return temp;
        }
    }
}
