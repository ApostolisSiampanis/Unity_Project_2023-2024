using Common.DialogueSystem;
using Common.InteractionSystem;
using UnityEngine;
using UnityEngine.Serialization;

namespace Forest
{
    public class MonologueTrigger : MonoBehaviour
    {
        public Interactor interactor;
        public Dialogue monologue;
        
        void Start()
        {
            interactor.TriggerMonologue(monologue);
        }
        
    }
}
