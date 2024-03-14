using UnityEngine;

namespace Common.DialogueSystem
{
    public class DialogueTrigger : MonoBehaviour
    {
        // ====== PUBLIC ====== //
        public Dialogue greetDialogue;

        // ====== PRIVATE ====== //
        private DialogueManager _dialogueManager;

        public void Start()
        {
            _dialogueManager = FindObjectOfType<DialogueManager>();
        }

        public void TriggerDialogue(Dialogue dialogue, ISpeak speaker)
        {
            _dialogueManager.StartDialogue(dialogue, speaker);
        }

        public void Greet(ISpeak speaker)
        {
            _dialogueManager.StartDialogue(greetDialogue, speaker);
        }

        public void Abort()
        {
            _dialogueManager.Abort();
        }
    }
}