using Common;
using Common.QuestSystem;
using Inventory;
using Save;
using Save.Data;
using UnityEngine;
using UnityEngine.Playables;

namespace Farm
{
    public class FarmLevelLoader : LevelLoader
    {
        [Header("Cutscenes")]
        public PlayableDirector entryCutscene;
        
        [Header("Quests")]
        public CollectQuest collectCarrotsQuest;
        public InteractQuest interactWithCarQuest;
        
        private FarmData _farmData;
    
        private void Start()
        {
            _farmData = SaveSystem.LoadFarmProgress();
            InitScene();
        }

        public override void InitScene()
        {
            if (_farmData == null)
            {
                return;
            }
            
            if (_farmData.questIndex >= collectCarrotsQuest.questIndex)
            {
                if (_farmData.carrotsVisibility == null) Debug.LogError("Retrieved Quest was CollectCarrotsQuest but carrot visibility was not retrieved.");
                else
                {
                    // Change visibility for each carrot
                    var carrots = collectCarrotsQuest.collectables;
                    for (var i = 0; i < carrots.Count; i++)
                    {
                        carrots[i].gameObject.SetActive(_farmData.carrotsVisibility[i]);
                    }
                }
            }
            
            if (_farmData.questIndex == interactWithCarQuest.questIndex)
            {
                // Give toolbox to the player
                interactor.inventory.AddItem(new Item()
                {
                    itemType = Item.ItemType.Toolbox,
                    amount = 1
                });
                
                // Enable cutscene for the fix watering system quest
                entryCutscene.enabled = true;
            }
            else
            {
                var playerPosition = _farmData.playerPosition;
                var vectorPlayerPosition = new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
                player.transform.position = vectorPlayerPosition;
            }
            
            Debug.Log("_farmData.questIndex= " + _farmData.questIndex);
            QuestManager.instance.SetCurrentQuestIndex(_farmData.questIndex);
        }
    }
}
