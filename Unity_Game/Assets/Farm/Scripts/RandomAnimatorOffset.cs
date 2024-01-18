using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomAnimatorOffset : MonoBehaviour
{
    [SerializeField] private string animationName;
    
    private Animator animator;
    private float randomOffset;
    
    // Start is called before the first frame update
    void Start()
    {
        // Get the animator component
        animator = GetComponent<Animator>();

        if (animator != null)
        {
            randomOffset = Random.Range(0f, 2f);
            
            animator.Play(animationName, 0, randomOffset);
        }
        else
        {
            // Log an error if Animator component is not found
            Debug.LogError("Animator component not found on " + gameObject.name);
        }

    }
}
