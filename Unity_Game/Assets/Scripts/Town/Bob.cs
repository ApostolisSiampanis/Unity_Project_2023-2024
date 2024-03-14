using Common.InteractionSystem;
using UnityEngine;

namespace Town
{
    public class Bob : NPC
    {
        protected override void ChangeState()
        {
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