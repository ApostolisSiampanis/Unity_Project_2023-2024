using System;
using System.Collections.Generic;
using UnityEngine;

namespace Farm.Scripts.DialogueSystem
{
    [CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue/Create new Dialogue")]
    public class Dialogue : ScriptableObject
    {
        public List<DialogueSentence> sentences;
    }
}