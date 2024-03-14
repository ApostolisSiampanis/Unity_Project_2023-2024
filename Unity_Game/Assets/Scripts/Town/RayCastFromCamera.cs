using UnityEngine;

namespace Town
{
    public class RayCastFromCamera : MonoBehaviour
    {
        [SerializeField] private Transform mainCameraTransform;
        private bool _mAlreadyWaved;

        private void Start()
        {
            mainCameraTransform = GameObject.FindWithTag("MainCamera").transform;
        }

        private void FixedUpdate()
        {
            Vector3 mainCameraPosition = mainCameraTransform.position;
            Vector3 direction = mainCameraTransform.forward;

            Debug.DrawRay(mainCameraPosition, direction * 7f, Color.green);
        }
    }
}