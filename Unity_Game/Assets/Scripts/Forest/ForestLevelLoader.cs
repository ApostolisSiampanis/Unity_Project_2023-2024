using Common;
using Common.QuestSystem;
using Save;
using Save.Data;
using UnityEngine;
using UnityEngine.Playables;

namespace Forest
{
    public class ForestLevelLoader : LevelLoader
    {
        [Header("Cutscenes")]
        public PlayableDirector farmForestCutscene;
        public PlayableDirector townFarmCutscene;

        [Header("Quests")] 
        public CollectQuest findBookQuest;
        public InteractQuest carRepairQuest;
        
        private ForestData _forestData;
    
        private void Start()
        {
            _forestData = SaveSystem.LoadForestProgress();
            InitScene();
        }

        public override void InitScene()
        {
            if (_forestData == null)
            {
                // Play first cutscene
                farmForestCutscene.enabled = true;
                return;
            }

            if (_forestData.questIndex >= findBookQuest.questIndex)
            {
                // Remove the book from the scene
                findBookQuest.collectables.ForEach(collectable => collectable.gameObject.SetActive(false));
                
                // Give the reward to the player
                interactor.inventory.AddItem(findBookQuest.Reward);
            }

            if (_forestData.questIndex == carRepairQuest.questIndex)
            {
                // Play Town -> Farm Cutscene since the last quest is complete
                townFarmCutscene.enabled = true;
            }
            else
            {
                var playerPosition = _forestData.playerPosition;
                var vectorPlayerPosition = new Vector3(playerPosition[0], playerPosition[1], playerPosition[2]);
                player.transform.position = vectorPlayerPosition;
            }
            
            Debug.Log("_forestData.questIndex= " + _forestData.questIndex);
            QuestManager.instance.SetCurrentQuestIndex(_forestData.questIndex);
        }
    }
}
