using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCPatrollingController : MonoBehaviour
{

    [SerializeField] private Transform[] waypoints;
    
    private int currentWaypointIdx;

    private NavMeshAgent agent;
    private Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        GoToNextWaypoint();
    }

    // Update is called once per frame
    void Update()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
            GoToNextWaypoint();
    }
    
    private void GoToNextWaypoint()
    {
        if (waypoints.Length == 0)
            return;
        
        agent.destination = waypoints[currentWaypointIdx].position;
        currentWaypointIdx = (currentWaypointIdx + 1) % waypoints.Length;
    }
    
}
