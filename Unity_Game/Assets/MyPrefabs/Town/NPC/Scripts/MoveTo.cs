// MoveTo.cs

using UnityEngine;
using UnityEngine.AI;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class MoveTo : MonoBehaviour
    {
        public Transform goal;
        private NavMeshAgent m_agent;
        public Animator animator;
        public NavMeshAgent navMeshAgent;
        private static readonly int IS_WALKING = Animator.StringToHash("isWalking");

        private void Start()
        {
            m_agent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            m_agent.destination = goal.position;
            animator.SetBool(IS_WALKING, navMeshAgent.hasPath);
        }
    }
}