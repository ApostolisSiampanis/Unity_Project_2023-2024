using System.Collections.Generic;
using Common.InteractionSystem;
using Inventory;
using Save;
using Save.Data;
using TMPro;
using UnityEngine;

namespace Common.QuestSystem
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public GameObject questPanel;
        public TextMeshProUGUI questTitleText;
        public TextMeshProUGUI questDescriptionText;

        public GameObject player;

        public List<Quest> quests;
        private int _currentQuestIdx;

        public Quest currentQuest;

        public void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                _currentQuestIdx = -1;
            
                NextQuest();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void UpdateQuestUI()
        {
            if (currentQuest == null)
            {
                questPanel.SetActive(false);
            }
            else
            {
                questPanel.SetActive(currentQuest.state == Quest.State.InProgress);
                questTitleText.text = quests[_currentQuestIdx].title;
                questDescriptionText.text = quests[_currentQuestIdx].description;
            }
        }

        public void StartQuest(NPC requester)
        {
            if (currentQuest.responsibleNPC != null && currentQuest.responsibleNPC != requester) return;
            if (requester != null) requester.ShowQuestHint(false);
            UpdateQuestUI();
        }

        public void CompleteQuest()
        {
            if (currentQuest.responsibleNPC != null) currentQuest.responsibleNPC.availableQuest = null;
            
            // Auto save quest
            AutoSave();
            
            NextQuest();
        }

        private void AutoSave()
        {
            var scene = currentQuest.scene;
            switch (scene)
            {
                case Quest.Scene.Farm:
                    List<Collectable> carrots = null;
                    
                    if (currentQuest is CollectQuest { itemType: Item.ItemType.Carrot } quest)
                    {
                        carrots = quest.collectables;
                    }
                    
                    SaveSystem.SaveFarmProgress(new FarmData(currentQuest.questIndex, player.transform.position , carrots));
                    break;
                case Quest.Scene.Forest:
                    Debug.Log("Forest quest");
                    break;
                case Quest.Scene.Town:
                    SaveSystem.SaveTownProgress(new TownData(currentQuest.questIndex, player.transform.position));
                    break;
            }
            
        }

        private void NextQuest()
        {
            if (++_currentQuestIdx >= quests.Count)
            {
                currentQuest = null;
                Debug.Log("No more available quests");
                UpdateQuestUI();
                return;
            }
        
            currentQuest = quests[_currentQuestIdx];

            if (currentQuest.responsibleNPC == null)
            {
                currentQuest.StartQuest(null);
            }
            else
            {
                currentQuest.responsibleNPC.availableQuest = currentQuest;
                currentQuest.responsibleNPC.ShowQuestHint(true);
            }
        
            UpdateQuestUI();
        }

        public void SetCurrentQuestIndex(int questIndex)
        {
            _currentQuestIdx = questIndex;
            NextQuest();
        }
    }
}
