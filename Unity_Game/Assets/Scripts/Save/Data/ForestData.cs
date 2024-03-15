using UnityEngine;

namespace Save.Data
{
    [System.Serializable]
    public class ForestData
    {
        public int questIndex { get; set; }
        public float[] playerPosition { get; set; }
        
        public ForestData(int questIndex, Vector3 playerPosition)
        {
            this.questIndex = questIndex;
            this.playerPosition = new[] { playerPosition.x, playerPosition.y, playerPosition.z };
        }
    }
}
