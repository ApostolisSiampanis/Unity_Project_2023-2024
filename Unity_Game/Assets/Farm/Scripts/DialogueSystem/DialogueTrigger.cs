using System.Collections.Generic;
using Farm.Scripts.Interaction_System;
using UnityEngine;

namespace Farm.Scripts.DialogueSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        // ====== PUBLIC ====== //
        public List<Dialogue> dialogues;

        // ====== PRIVATE ====== //
        private DialogueManager _dialogueManager;

        public void Start()
        {
            _dialogueManager = FindObjectOfType<DialogueManager>();
        }

        public void TriggerDialogue(NPCSpeaker npcSpeaker)
        {
            _dialogueManager.StartDialogue(dialogues[0], npcSpeaker);
        }

        public void Abort()
        {
            _dialogueManager.Abort();
        }
    }
}