using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoToDestination : MonoBehaviour
{
    public Transform destination;

    private UnityEngine.AI.NavMeshAgent navMeshAgent;
    private Animator animator;

    void Awake()
    {
        navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        animator = GetComponent<Animator>();

        navMeshAgent.SetDestination(destination.position);
    }

    void Update()
    {
        float speed = navMeshAgent.velocity.magnitude / navMeshAgent.speed;
        animator.SetFloat("Speed", speed);
    }
}
