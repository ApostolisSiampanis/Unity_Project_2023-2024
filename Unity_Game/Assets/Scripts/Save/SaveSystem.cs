using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Save.Data;

namespace Save
{
    public static class SaveSystem
    {
        public static void SaveSettings(SettingsData settingsData)
        {
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + "/settings.save";
            
            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, settingsData);
                Debug.Log("Settings have been saved at " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving settings: " + e.Message);
            }
            
        }

        public static SettingsData LoadSettings()
        {
            var path = Application.persistentDataPath + "/settings.save";
            try
            {
                if (File.Exists(path))
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as SettingsData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading settings: " + e.Message);
            }
            
            Debug.Log("Save file not found in " + path);
            return null;
        }
    }
}
