using UnityEngine;
using UnityEngine.AI;

namespace Town
{
    public class MoveTo : MonoBehaviour
    {
        [SerializeField] private Transform goal;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private Animator animator;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");

        private void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
        }

        private void Update()
        {
            agent.destination = goal.position;
            animator.SetBool(IS_WALKING, agent.hasPath);
        }
    }
}