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

    public GameObject questPanel;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;

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

    public void AcceptQuest(NPC requester)
    {
        if (currentQuest.responsibleNPC != requester) return;
        currentQuest.StartQuest();
        requester.ShowQuestHint(false);
        UpdateQuestUI();
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
            currentQuest.StartQuest();
        }
        else
        {
            currentQuest.responsibleNPC.availableQuest = currentQuest;
            currentQuest.responsibleNPC.ShowQuestHint(true);
        }
        
        UpdateQuestUI();
    }
}
