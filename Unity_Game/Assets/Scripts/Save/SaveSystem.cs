using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using Save.Data;

namespace Save
{
    public static class SaveSystem
    {
        private const string SETTINGS_PATH = "/settings.save";
        private const string LOCALE_PATH = "/locale.save";
        
        public static void SaveSettings(SettingsData settingsData)
        {
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + SETTINGS_PATH;
            
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
            var path = Application.persistentDataPath + SETTINGS_PATH;
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
            
            Debug.Log("Settings save file not found in " + path);
            return null;
        }

        public static void SaveLocale(int localeId)
        {
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + LOCALE_PATH;
            
            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, localeId);
                Debug.Log("Locale has been saved at " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving locale: " + e.Message);
            }
            
        }
        
        public static int LoadLocale()
        {
            var path = Application.persistentDataPath + LOCALE_PATH;
            try
            {
                if (File.Exists(path))
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    var formatter = new BinaryFormatter();
                    return (int) formatter.Deserialize(stream);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading locale: " + e.Message);
            }
            
            Debug.Log("Locale save file not found in " + path);
            return -1;
        }
    }
}
