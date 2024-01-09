// Patrol.cs

using UnityEngine;
using UnityEngine.AI;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class PolicePatrol : MonoBehaviour
    {
        public Transform[] points;
        private int _destPoint;
        public NavMeshAgent agent;
        public Animator animator;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");

        // Minimum and maximum durations for the walking animation
        private float _minWalkDuration = 5.0f;
        private float _maxWalkDuration = 10.0f;

        // Timers for tracking the current state
        private float _breathingTimer = 5f;
        private float _walkingTimer;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();

            // Initialize the walking timer
            SetRandomWalkingDuration();

            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            agent.autoBraking = false;

            GotoNextPoint();
        }

        private void GotoNextPoint()
        {
            // Returns if no points have been set up
            if (points.Length == 0)
                return;

            // Set the agent to go to the currently selected destination.
            agent.destination = points[_destPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            _destPoint = (_destPoint + 1) % points.Length;
        }

        private void Update()
        {
            if (animator.GetBool(IS_WALKING))
            {
                _walkingTimer -= Time.deltaTime;

                if (_walkingTimer <= 0)
                {
                    animator.SetBool(IS_WALKING, false);
                    agent.isStopped = true;
                    agent.autoBraking = true;
                    return;
                }

                // Choose the next destination point when the agent gets
                // close to the current one.
                if (!agent.pathPending && agent.remainingDistance < 0.5f)
                    GotoNextPoint();
            }
            else
            {
                _breathingTimer -= Time.deltaTime;

                if (!(_breathingTimer <= 0)) return;
                animator.SetBool(IS_WALKING, true);
                agent.isStopped = false;
                agent.autoBraking = false;
                _breathingTimer = 5f;
                SetRandomWalkingDuration();
            }
        }

        // Set a random duration for the walking state
        private void SetRandomWalkingDuration()
        {
            _walkingTimer = Random.Range(_minWalkDuration, _maxWalkDuration);
        }
    }
}