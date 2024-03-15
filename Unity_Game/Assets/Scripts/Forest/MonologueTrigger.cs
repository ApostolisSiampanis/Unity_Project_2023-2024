using System.Collections.Generic;
using Common.DialogueSystem;
using Common.InteractionSystem;
using UnityEngine;

namespace Forest
{
    public class MonologueTrigger : MonoBehaviour
    {
        public Interactor interactor;
        [SerializeField] private OnCarBreak onCarBreak;

        private void Start()
        {
            Dialogue monologue = new()
            {
                sentences = new List<DialogueSentence>
                {
                    new("Alex", onCarBreak.GetMonologue())
                }
            };
            interactor.TriggerMonologue(monologue);
        }
    }
}