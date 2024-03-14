namespace Save.Data
{
    [System.Serializable]
    public class ProgressData
    {
        public int sceneIndex { get; set; }
        public int questIndex { get; set; }
        public float[] playerPosition { get; set; } 
        public bool[] carrotVisibility { get; set; }
        public bool bookVisibility { get; set; }
        

        public ProgressData(int sceneIndex, int questIndex, float[] playerPosition, bool[] carrotVisibility, bool bookVisibility)
        {
            this.sceneIndex = sceneIndex;
            this.questIndex = questIndex;
            this.playerPosition = playerPosition;
            this.carrotVisibility = carrotVisibility;
            this.bookVisibility = bookVisibility;
        }
    }
}
