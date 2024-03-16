using Common.InteractionSystem;
using UnityEngine;

namespace Farm
{
    public class Alice : NPC
    {
        private RandomTreeCollector _randomTreeCollector;
        private static readonly int IS_IDLE = Animator.StringToHash("is_idle");
        private static readonly int PICK_FRUIT = Animator.StringToHash("pick_fruit");

        public new void Start()
        {
            base.Start();
            _randomTreeCollector = GetComponent<RandomTreeCollector>();

            // Check if necessary objects are present
            if (_randomTreeCollector == null) Debug.LogError("RandomTreeCollector is missing");
        }

        protected override void ChangeState()
        {
            Animator.SetBool(IS_IDLE, IsTalking);
            NavMeshAgent.isStopped = IsTalking;

            if (IsTalking)
            {
                // Change orientation to look at the interactor
                PrevRotation = transform.rotation;
                var direction = Interactor.transform.position - transform.position;
                transform.rotation = Quaternion.LookRotation(direction);
            }
            else
            {
                // Change the orientation back to normal
                transform.rotation = PrevRotation;

                // If the NPC was collecting fruit, continue doing so
                if (_randomTreeCollector.collectingFruit)
                {
                    Animator.SetTrigger(PICK_FRUIT);
                }

                // Continue walking
                Animator.SetBool(IS_IDLE, false);
            }
        }
    }
}