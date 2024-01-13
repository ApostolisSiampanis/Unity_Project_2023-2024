using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class RandomTreeCollector : MonoBehaviour
{

    [SerializeField] private Transform[] treePositions;
    
    private Animator animator;
    private NavMeshAgent navMeshAgent;
    private Transform currentTarget;

    private bool collectingFruit;
    public int collectedFruits = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        ChooseRandomTree();
        collectingFruit = false;
    }
    
    // Update is called once per frame
    void Update()
    {
        // Check if the NPC has reached the target tree
        if (!collectingFruit && navMeshAgent.hasPath && navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // Look at the tree
            Vector3 targetPosition = new Vector3(currentTarget.position.x, transform.position.y, currentTarget.position.z);
            transform.LookAt(targetPosition);
            
            // Play the animation
            animator.SetTrigger("pick_fruit");
            collectingFruit = true;

        }
    }

    public void OnEndOfFruitPickAnimation()
    {
        collectingFruit = false;
        collectedFruits += 3;
        
        ChooseRandomTree();
    }
    
    private void ChooseRandomTree()
    {
        // Choose a random tree position different from the current one
        var newTarget = GetRandomTreePosition();
        while (newTarget == currentTarget)
        {
            newTarget = GetRandomTreePosition();
        }
        
        // Set the new target and start moving towards it
        currentTarget = newTarget;
        navMeshAgent.SetDestination(currentTarget.position);
        
        // Play walk animation
    }

    private Transform GetRandomTreePosition()
    {
        // Return a random tree position
        return treePositions[Random.Range(0, treePositions.Length)];
    }
}
