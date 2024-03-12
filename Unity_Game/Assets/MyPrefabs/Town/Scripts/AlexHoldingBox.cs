using StarterAssets.ThirdPersonController.Scripts;
using UnityEngine;

namespace MyPrefabs.Town.Scripts
{
    public class AlexHoldingBox : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private ThirdPersonController thirdPersonController;
        private float _initialSprintSpeed;
        private static readonly int HOLDING_BOX_IDLE = Animator.StringToHash("HoldingBoxIdle");
        private static readonly int HOLDING_BOX_WALK = Animator.StringToHash("HoldingBoxWalk");

        private void Start()
        {
            _initialSprintSpeed = thirdPersonController.SprintSpeed;
        }

        private void Update()
        {
            // if player is not holding the box, reset the sprint speed and return.
            if (!animator.GetBool(HOLDING_BOX_IDLE))
            {
                thirdPersonController.SprintSpeed = _initialSprintSpeed;
                return;
            }

            // Otherwise, check if the player is moving.
            if (
                // A pressed but not D
                (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D)) ||
                // D pressed but not A
                (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A)) ||
                // W pressed but not S
                (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S)) ||
                // S pressed but not W
                (Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.W))
            )
            {
                // If player is moving, set the animator to walk and set the sprint speed to 3.5f.
                animator.SetBool(HOLDING_BOX_WALK, true);
                thirdPersonController.SprintSpeed = 3.5f;
            }
            else
            {
                // If player is not moving, set the animator to idle.
                animator.SetBool(HOLDING_BOX_WALK, false);
            }
        }
    }
}