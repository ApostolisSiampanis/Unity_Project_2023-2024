using UnityEngine;

namespace Farm
{
    public class RandomAnimatorOffset : MonoBehaviour
    {
        [SerializeField] private string animationName;

        private Animator _animator;
        private float _randomOffset;

        // Start is called before the first frame update
        private void Start()
        {
            // Get the animator component
            _animator = GetComponent<Animator>();

            if (_animator != null)
            {
                _randomOffset = Random.Range(0f, 2f);

                _animator.Play(animationName, 0, _randomOffset);
            }
            else
            {
                // Log an error if Animator component is not found
                Debug.LogError("Animator component not found on " + gameObject.name);
            }
        }
    }
}