using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

namespace Farm
{
    public class RandomTreeCollector : MonoBehaviour
    {
        [SerializeField] private Transform[] treePositions;
        [SerializeField] private int fruitsPerTree;

        private Animator _animator;
        private NavMeshAgent _navMeshAgent;
        private Transform _currentTarget;

        public bool collectingFruit;

        // Start is called before the first frame update
        private void Start()
        {
            _animator = GetComponent<Animator>();
            _navMeshAgent = GetComponent<NavMeshAgent>();

            ChooseRandomTree();
            collectingFruit = false;
        }

        // Update is called once per frame
        private void Update()
        {
            // Check if the NPC has reached the target tree
            if (!collectingFruit && _navMeshAgent.hasPath &&
                _navMeshAgent.remainingDistance <= _navMeshAgent.stoppingDistance)
            {
                // Look at the tree
                Vector3 targetPosition =
                    new Vector3(_currentTarget.position.x, transform.position.y, _currentTarget.position.z);
                transform.LookAt(targetPosition);

                // Play the animation
                _animator.SetTrigger("pick_fruit");
                collectingFruit = true;
            }
        }

        public void OnEndOfFruitPickAnimation()
        {
            collectingFruit = false;
            ChooseRandomTree();
        }

        private void ChooseRandomTree()
        {
            // Choose a random tree position different from the current one
            var newTarget = GetRandomTreePosition();
            while (newTarget == _currentTarget)
            {
                newTarget = GetRandomTreePosition();
            }

            // Set the new target and start moving towards it
            _currentTarget = newTarget;
            _navMeshAgent.SetDestination(_currentTarget.position);

            // Play walk animation
        }

        private Transform GetRandomTreePosition()
        {
            // Return a random tree position
            return treePositions[Random.Range(0, treePositions.Length)];
        }
    }
}