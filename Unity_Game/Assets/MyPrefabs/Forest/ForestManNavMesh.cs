using UnityEngine;
using UnityEngine.AI;

namespace MyPrefabs.Forest
{
    public class ForestManNavMesh : MonoBehaviour
    {
        [SerializeField] private Transform movePositionTransform;
    
        private NavMeshAgent navMeshAgent;

        private void Awake()
        {
            navMeshAgent = GetComponent<NavMeshAgent>();
        }

        private void Update()
        {
            navMeshAgent.destination = movePositionTransform.position;
        }
    }
}
