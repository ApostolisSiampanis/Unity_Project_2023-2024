using UnityEngine;

namespace Town
{
    public class RayCast : MonoBehaviour
    {
        [SerializeField] private Transform alexTransform;
        [SerializeField] private Animator npcAnimator;
        private static readonly int WAVE = Animator.StringToHash("wave");
        private Vector3 _mNpcEyesPosition, _mAlexEyesPosition;
        private bool _mAlreadyWaved;

        private void Start()
        {
            npcAnimator = GetComponent<Animator>();
            alexTransform = GameObject.FindWithTag("Player").transform;
            _mNpcEyesPosition = transform.position + new Vector3(0, 1.7f, 0);
        }

        private void FixedUpdate()
        {
            _mAlexEyesPosition = alexTransform.position + new Vector3(0, 1.65f, 0);

            Vector3 direction = _mAlexEyesPosition - _mNpcEyesPosition;

            Debug.DrawRay(_mNpcEyesPosition, direction, Color.yellow);

            if (!Physics.Raycast(_mNpcEyesPosition, direction, out var hit, 10f)) return;
            if (hit.collider.transform == alexTransform)
            {
                if (_mAlreadyWaved) return;
                npcAnimator.SetTrigger(WAVE);
                _mAlreadyWaved = true;
            }
            else _mAlreadyWaved = false;
        }
    }
}