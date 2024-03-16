using UnityEngine;
using UnityEngine.AI;

namespace Farm
{
    public class NPCPatrollingController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;

        private int _currentWaypointIdx;

        private NavMeshAgent _agent;
        private Animator _animator;

        // Start is called before the first frame update
        private void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _animator = GetComponent<Animator>();

            GoToNextWaypoint();
        }

        // Update is called once per frame
        private void Update()
        {
            if (!_agent.pathPending && _agent.remainingDistance < 0.5f)
                GoToNextWaypoint();
        }

        private void GoToNextWaypoint()
        {
            if (waypoints.Length == 0)
                return;

            _agent.destination = waypoints[_currentWaypointIdx].position;
            _currentWaypointIdx = (_currentWaypointIdx + 1) % waypoints.Length;
        }
    }
}