using UnityEngine;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class RayCast : MonoBehaviour
    {
        [SerializeField] private Transform alexTransform;
        [SerializeField] private Animator npcAnimator;
        private static readonly int WAVE = Animator.StringToHash("wave");
        private Vector3 m_npcPosition, m_npcEyesPosition;
        private bool m_alreadyWaved;

        private void Start()
        {
            npcAnimator = GetComponent<Animator>();
            alexTransform = GameObject.FindWithTag("Player").transform;
            m_npcPosition = transform.position + new Vector3(0, 1.7f, 0);
        }

        private void FixedUpdate()
        {
            m_npcEyesPosition = alexTransform.position + new Vector3(0, 1.65f, 0);

            Vector3 direction = m_npcEyesPosition - m_npcPosition;

            Debug.DrawRay(m_npcPosition, direction, Color.yellow);

            if (!Physics.Raycast(m_npcPosition, direction, out var hit, 10f)) return;
            if (hit.collider.transform == alexTransform)
            {
                if (m_alreadyWaved) return;
                npcAnimator.SetTrigger(WAVE);
                m_alreadyWaved = true;
            }
            else m_alreadyWaved = false;
        }
    }
}