using UnityEngine;

namespace MyPrefabs.Town.NPC.FastFoodGuy
{
    public class RayCast : MonoBehaviour
    {
        [SerializeField] private Transform alexTransform;
        private Animator m_fastFoodGuyAnimator;
        private static readonly int WAVE = Animator.StringToHash("wave");
        private Vector3 m_fastFoodGuyEyesPosition, m_alexEyesPosition;
        private bool m_alreadyWaved;

        private void Start()
        {
            m_fastFoodGuyAnimator = GetComponent<Animator>();
            alexTransform = GameObject.FindWithTag("Player").transform;
            m_fastFoodGuyEyesPosition = transform.position + new Vector3(0, 1.7f, 0);
        }

        private void FixedUpdate()
        {
            m_alexEyesPosition = alexTransform.position + new Vector3(0, 1.65f, 0);

            var direction = m_alexEyesPosition - m_fastFoodGuyEyesPosition;

            Debug.DrawRay(m_fastFoodGuyEyesPosition, direction, Color.yellow);

            if (!Physics.Raycast(m_fastFoodGuyEyesPosition, direction, out var hit, 10f)) return;
            if (hit.collider.transform == alexTransform)
            {
                if (m_alreadyWaved) return;
                m_fastFoodGuyAnimator.SetTrigger(WAVE);
                m_alreadyWaved = true;
            }
            else m_alreadyWaved = false;
        }
    }
}