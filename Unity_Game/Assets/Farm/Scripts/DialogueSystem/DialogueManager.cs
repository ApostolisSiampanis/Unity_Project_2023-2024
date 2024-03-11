using TMPro;
using UnityEngine;

namespace Farm.Scripts.DialogueSystem
{
    public class DialogueManager : MonoBehaviour
    {
        // ====== PUBLIC ====== //
        public GameObject dialogueGameObject;
        public TextMeshProUGUI speakerNameText;
        public TextMeshProUGUI bodyText;
    
        // ====== PRIVATE ====== //
        private Dialogue _dialogue;
        private ISpeak _currentSpeaker;
        private CursorLockMode _cursorPrevState;
        private int _currentSentenceIdx;

        public void Start()
        {
            if (dialogueGameObject == null) Debug.LogError("Dialogue object is missing");
            else dialogueGameObject.SetActive(false);
        
            if (speakerNameText == null) Debug.LogError("Speaker Name Text object is missing");
            if (bodyText == null) Debug.LogError("Body Text object is missing");
        }

        public void StartDialogue(Dialogue dialogue, ISpeak speaker)
        {
            _dialogue = dialogue;
            _currentSpeaker = speaker;
            _currentSentenceIdx = 0;

            _cursorPrevState = Cursor.lockState;
            Cursor.lockState = CursorLockMode.None;
        
            Debug.Log("[DialogueManager]: New dialogue has been assigned");
            DisplayNextSentence();
            dialogueGameObject.SetActive(true);
        }

        public void DisplayNextSentence()
        {
            if (_currentSentenceIdx >= _dialogue.sentences.Count)
            {
                EndDialogue(true);
                return;
            }
        
            var sentence = _dialogue.sentences[_currentSentenceIdx++];
            speakerNameText.text = sentence.name;
            bodyText.text = sentence.text;
        }

        private void EndDialogue(bool isFinished)
        {
            Cursor.lockState = _cursorPrevState;
        
            dialogueGameObject.SetActive(false);
            _currentSpeaker.OnDialogueEnd(isFinished);
        
            Debug.Log("End of dialogue");
        }

        public void Abort()
        {
            EndDialogue(false);
        }
    }
}
