using UnityEngine;

namespace Save.Data
{
    [System.Serializable]
    public class SettingsData
    {
        public float soundsVolume { get; set; }
        public float musicVolume { get; set; }
        public int qualityIndex { get; set; }
        public bool isFullscreen { get; set; }
        public int resolutionIndex { get; set; }

        public SettingsData(float soundsVolume, float musicVolume, int qualityIndex, bool isFullscreen, int resolutionIndex)
        {
            this.soundsVolume = soundsVolume;
            this.musicVolume = musicVolume;
            this.qualityIndex = qualityIndex;
            this.isFullscreen = isFullscreen;
            this.resolutionIndex = resolutionIndex;
        }
    }
}
