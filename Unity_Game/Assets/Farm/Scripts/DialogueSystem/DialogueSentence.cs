using System;
using UnityEngine;

namespace Farm.Scripts.DialogueSystem
{
    [Serializable]
    public class DialogueSentence
    {
        public string name;
        [TextArea(3, 10)]
        public string text;
    }
}