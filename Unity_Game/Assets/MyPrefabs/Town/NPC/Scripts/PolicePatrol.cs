// Patrol.cs

using UnityEngine;
using UnityEngine.AI;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class PolicePatrol : MonoBehaviour
    {
        public Transform[] points;
        private int m_destPoint;
        public NavMeshAgent agent;
        public Animator animator;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");

        // Minimum and maximum durations for the walking animation
        private const float MIN_WALK_DURATION = 5.0f;
        private const float MAX_WALK_DURATION = 10.0f;

        // Timers for tracking the current state
        private float m_breathingTimer = 5f;
        private float m_walkingTimer;

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
            agent.destination = points[m_destPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            m_destPoint = (m_destPoint + 1) % points.Length;
        }

        private void Update()
        {
            if (animator.GetBool(IS_WALKING))
            {
                m_walkingTimer -= Time.deltaTime;

                if (m_walkingTimer <= 0)
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
                m_breathingTimer -= Time.deltaTime;

                if (!(m_breathingTimer <= 0)) return;
                animator.SetBool(IS_WALKING, true);
                agent.isStopped = false;
                agent.autoBraking = false;
                m_breathingTimer = 5f;
                SetRandomWalkingDuration();
            }
        }

        // Set a random duration for the walking state
        private void SetRandomWalkingDuration()
        {
            m_walkingTimer = Random.Range(MIN_WALK_DURATION, MAX_WALK_DURATION);
        }
    }
}