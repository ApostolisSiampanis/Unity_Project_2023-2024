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
        
        private const string FARM_PROGRESS_PATH = "/farm_progress.save";
        private const string TOWN_PROGRESS_PATH = "/town_progress.save";
        
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

        public static void SaveProgress(ProgressData progressData)
        {
            // var formatter = new BinaryFormatter();
            // var path = Application.persistentDataPath + PROGRESS_PATH;
            //
            // try
            // {
            //     using var stream = new FileStream(path, FileMode.Create);
            //     formatter.Serialize(stream, progressData);
            //     Debug.Log("Progress has been saved at " + path);
            // }
            // catch (System.Exception e)
            // {
            //     Debug.LogError("Error saving progress: " + e.Message);
            // }
        }

        public static void SaveTownProgress(TownData townData)
        {
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + TOWN_PROGRESS_PATH;
            
            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, townData);
                Debug.Log("Town progress has been saved at " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving town progress: " + e.Message);
            }
        }
        
        public static TownData LoadTownProgress()
        {
            var path = Application.persistentDataPath + TOWN_PROGRESS_PATH;
            try
            {
                if (File.Exists(path))
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as TownData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading town progress: " + e.Message);
            }
            
            Debug.Log("Town progress save file not found in " + path);
            return null;
        }
        
        public static void SaveFarmProgress(FarmData farmData)
        {
            var formatter = new BinaryFormatter();
            var path = Application.persistentDataPath + FARM_PROGRESS_PATH;
            
            try
            {
                using var stream = new FileStream(path, FileMode.Create);
                formatter.Serialize(stream, farmData);
                Debug.Log("Farm progress has been saved at " + path);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error saving farm progress: " + e.Message);
            }
        }
        
        public static FarmData LoadFarmProgress()
        {
            var path = Application.persistentDataPath + FARM_PROGRESS_PATH;
            try
            {
                if (File.Exists(path))
                {
                    using var stream = new FileStream(path, FileMode.Open);
                    var formatter = new BinaryFormatter();
                    return formatter.Deserialize(stream) as FarmData;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading farm progress: " + e.Message);
            }
            
            Debug.Log("Farm progress save file not found in " + path);
            return null;
        }
    }
}
