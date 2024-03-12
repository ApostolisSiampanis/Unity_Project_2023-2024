using System;
using System.Collections.Generic;

namespace Farm.Scripts.DialogueSystem
{
    [Serializable]
    public class Dialogue
    {
        public int id;
        public List<DialogueSentence> sentences;
    }
}