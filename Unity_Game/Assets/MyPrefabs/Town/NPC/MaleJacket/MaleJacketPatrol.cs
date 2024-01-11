// Patrol.cs

using UnityEngine;
using UnityEngine.AI;

namespace MyPrefabs.Town.NPC.MaleJacket
{
    public class MaleJacketPatrol : MonoBehaviour
    {
        public Transform[] points;
        private int m_destPoint;
        private static NavMeshAgent _agent;
        private static Animator _animator;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");
        public static bool gatheringAnimationPlaying;

        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            // Disabling auto-braking allows for continuous movement
            // between points (ie, the agent doesn't slow down as it
            // approaches a destination point).
            _agent.autoBraking = false;

            GotoNextPoint();
        }

        private void GotoNextPoint()
        {
            // Returns if no points have been set up
            if (points.Length == 0)
                return;

            // Set the agent to go to the currently selected destination.
            _agent.destination = points[m_destPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            m_destPoint = (m_destPoint + 1) % points.Length;
        }

        private void Update()
        {
            if (gatheringAnimationPlaying) return;

            if (_agent.remainingDistance < 0.5f)
            {
                gatheringAnimationPlaying = true;
                _agent.isStopped = true;
                _agent.autoBraking = true;
                _animator.SetBool(IS_WALKING, false);
            }

            // Choose the next destination point when the agent gets
            // close to the current one.
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                GotoNextPoint();
        }

        public static void ResumeWalking()
        {
            _agent.isStopped = false;
            _agent.autoBraking = false;
            _animator.SetBool(IS_WALKING, true);
        }
    }
}