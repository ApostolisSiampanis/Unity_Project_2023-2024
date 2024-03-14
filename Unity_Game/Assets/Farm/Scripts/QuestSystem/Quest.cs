using Farm.Scripts.DialogueSystem;
using Farm.Scripts.InteractionSystem;
using UnityEngine;

namespace Farm.Scripts.QuestSystem
{
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

        [Header("Dialogues")] public Dialogue introDialogue;

        public Dialogue inProgressDialogue;
        public Dialogue onCompletionDialogue;

        protected int currentAmount;

        public void StartQuest(NPC requester)
        {
            SetupQuest();
            if (requester != null) requester.ShowQuestHint(false);
            QuestManager.Instance.StartQuest(requester);
        }

        protected abstract void SetupQuest();

        public bool IsCompleted()
        {
            return state == State.Completed;
        }

        public abstract void CompleteQuest(Interactor interactor, NPC requester);
        protected abstract void CheckObjective();
    }
}