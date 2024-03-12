using System.Collections;
using System.Collections.Generic;
using Farm.Scripts.DialogueSystem;
using Farm.Scripts.Interaction_System;
using UnityEngine;

public abstract class Quest : MonoBehaviour
{
    public enum State
    {
        NotStarted,
        InProgress,
        Completed
    }

    public string title;
    public string description;
    public int requiredAmount;
    public State state = State.NotStarted;

    public NPC responsibleNPC;

    [Header("Dialogues")]
    public Dialogue introDialogue;

    public Dialogue inProgressDialogue;
    public Dialogue onCompletionDialogue;

    protected int currentAmount;

    public abstract void StartQuest();
    protected abstract void SetupQuest();
    
    public abstract bool IsCompleted();
    public abstract void CompleteQuest(Interactor interactor);
    protected abstract void CheckObjective();
}
