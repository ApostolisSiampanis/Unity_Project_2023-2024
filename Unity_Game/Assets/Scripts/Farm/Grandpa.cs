using Common.InteractionSystem;
using UnityEngine;

namespace Farm
{
    public class Grandpa : NPC
    {
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");
        protected override void ChangeState()
        {
            Animator.SetBool(IS_WALKING, !IsTalking);
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
            }
        }
    }
}
