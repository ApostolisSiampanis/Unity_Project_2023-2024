using System.Collections;
using System.Collections.Generic;
using Common;
using Save;
using Save.Data;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainLevelLoader : LevelLoader
{

    public override void InitScene()
    {
        // Delete all progress save files and start a new game
        var progressSuccessfullyDeleted = SaveSystem.DeleteProgressFiles();
        if (!progressSuccessfullyDeleted)
        {
            Debug.LogError("Unable to Start a new game since something went wrong when deleting save files");
            return;
        }
        
        SaveSystem.SaveMainProgress(new MainProgressData((int)Scene.Farm));
        
        LoadScene((int)Scene.Farm);
    }
}
