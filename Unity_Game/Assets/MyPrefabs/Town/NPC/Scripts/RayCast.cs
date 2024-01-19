using UnityEngine;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class RayCast : MonoBehaviour
    {
        [SerializeField] private Transform alexTransform;
        [SerializeField] private Animator npcAnimator;
        private static readonly int WAVE = Animator.StringToHash("wave");
        private Vector3 m_npcEyesPosition, m_alexEyesPosition;
        private bool m_alreadyWaved;

        private void Start()
        {
            npcAnimator = GetComponent<Animator>();
            alexTransform = GameObject.FindWithTag("Player").transform;
            m_npcEyesPosition = transform.position + new Vector3(0, 1.7f, 0);
        }

        private void FixedUpdate()
        {
            m_alexEyesPosition = alexTransform.position + new Vector3(0, 1.65f, 0);

            Vector3 direction = m_alexEyesPosition - m_npcEyesPosition;

            Debug.DrawRay(m_npcEyesPosition, direction, Color.yellow);

            if (!Physics.Raycast(m_npcEyesPosition, direction, out var hit, 10f)) return;
            if (hit.collider.transform == alexTransform)
            {
                if (m_alreadyWaved) return;
                transform.LookAt(alexTransform);
                npcAnimator.SetTrigger(WAVE);
                m_alreadyWaved = true;
            }
            else m_alreadyWaved = false;
        }
    }
}