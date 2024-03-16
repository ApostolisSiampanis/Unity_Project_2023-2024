using UnityEngine;
using UnityEngine.AI;

namespace Town
{
    public class Patrol : MonoBehaviour
    {
        [SerializeField] private Transform[] points;
        [SerializeField] private NavMeshAgent agent;
        private int _mDestPoint;

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();

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
            agent.destination = points[_mDestPoint].position;

            // Choose the next point in the array as the destination,
            // cycling to the start if necessary.
            _mDestPoint = (_mDestPoint + 1) % points.Length;
        }

        private void Update()
        {
            // Choose the next destination point when the agent gets
            // close to the current one.
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
                GotoNextPoint();
        }
    }
}