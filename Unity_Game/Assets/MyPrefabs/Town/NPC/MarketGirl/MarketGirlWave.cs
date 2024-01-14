using UnityEngine;

namespace MyPrefabs.Town.NPC.MarketGirl
{
    public class MarketGirlWave : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        private static readonly int WAVE = Animator.StringToHash("wave");

        private void Start()
        {
            animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            animator.SetTrigger(WAVE);
        }
    }
}