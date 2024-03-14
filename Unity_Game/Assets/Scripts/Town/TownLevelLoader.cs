using System;
using Common;
using Common.QuestSystem;
using Inventory;
using Save;
using Save.Data;
using UnityEngine;
using UnityEngine.Playables;

namespace Town
{
    public class TownLevelLoader : LevelLoader
    {
        [Header("Cutscenes")]
        public PlayableDirector entryCutscene;
        
        [Header("Quests")]
        public InteractQuest talkToUncleQuest;
        public CarryQuest truckUnloadingQuest;
        public CollectQuest findToolBoxQuest;

        private TownData _townData;
        
        public void Start()
        {
            _townData = SaveSystem.LoadTownProgress();
            InitScene();
        }
        
        protected override void InitScene()
        {
            if (_townData == null)
            {
                entryCutscene.enabled = true;
                return;
            }
            
            if (_townData.questIndex >= 0)
            {
                // Do nothing    
            }

            if (_townData.questIndex >= 1)
            {
                // Remove the boxes from the truck
                truckUnloadingQuest.itemsToBeCarried.ForEach(item => item.gameObject.SetActive(false));
                // Show the boxes inside the shop
                truckUnloadingQuest.placementHints.ForEach(hint =>
                {
                    hint.objectToActivate.SetActive(true);
                    hint.gameObject.SetActive(false);
                });
            }

            if (_townData.questIndex >= 2)
            {
                // Remove toolbox
                findToolBoxQuest.collectables.ForEach(collectable => collectable.gameObject.SetActive(false));
                // Give toolbox to the player
                interactor.inventory.AddItem(findToolBoxQuest.Reward);
            }
            
            var playerPosition = _townData.playerPosition;
            var vectorPlayerPosition = new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
            interactor.transform.position = vectorPlayerPosition;
            Debug.Log("_TownData.questIndex= " + _townData.questIndex);
            QuestManager.Instance.SetCurrentQuestIndex(_townData.questIndex);
            
        }
    }
}
