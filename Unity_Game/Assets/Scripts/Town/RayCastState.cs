using UnityEngine;

namespace Town
{
    public class RayCastState : MonoBehaviour
    {
        [SerializeField] private GameObject alex;
        [SerializeField] private RayCast rayCast;

        private void Update()
        {
            rayCast.enabled = alex.activeSelf;
        }
    }
}