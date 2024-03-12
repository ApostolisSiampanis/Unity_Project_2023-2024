using System;
using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    // public TextMeshProUGUI objectiveText;

    public List<Quest> quests;
    private int _currentQuestIdx;

    public Quest currentQuest;

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            _currentQuestIdx = 0;
            currentQuest = quests[_currentQuestIdx];

            // Assuming responsibleNPC is a public field, you can set the available quest here
            currentQuest.responsibleNPC.availableQuest = currentQuest;
            // UpdateQuestUI();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void UpdateQuestUI()
    {
        questTitleText.text = quests[_currentQuestIdx].title;
        questDescriptionText.text = quests[_currentQuestIdx].description;
    }

    public void CompleteQuest(Interactor interactor, NPC requester)
    {
        if (requester != currentQuest.responsibleNPC || currentQuest.state != Quest.State.Completed)
        {
            Debug.LogError("Wrong call on QuestCompleted");
            return;
        }
        currentQuest.CompleteQuest(interactor);
        requester.availableQuest = null;
        NextQuest();
    }

    private void NextQuest()
    {
        Debug.Log("Next quest");
    }
}
