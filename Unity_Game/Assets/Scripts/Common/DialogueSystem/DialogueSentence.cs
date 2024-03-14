using System;
using UnityEngine;

namespace Common.DialogueSystem
{
    [Serializable]
    public class DialogueSentence
    {
        public string name;
        [TextArea(3, 10)] public string text;

        public DialogueSentence(string name, string text)
        {
            this.name = name;
            this.text = text;
        }
    }
}