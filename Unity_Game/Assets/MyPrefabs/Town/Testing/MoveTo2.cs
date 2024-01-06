// MoveTo.cs

using UnityEngine;
using UnityEngine.AI;

public class MoveTo2 : MonoBehaviour
{
    public Transform goal;
    private NavMeshAgent _agent;
    public Animator animator;
    public NavMeshAgent navMeshAgent;
    private static readonly int IsWalking = Animator.StringToHash("isWalking");

    private void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        _agent.destination = goal.position;
        animator.SetBool(IsWalking, navMeshAgent.hasPath);
    }
}