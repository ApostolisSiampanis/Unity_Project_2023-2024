using UnityEngine;

namespace MyPrefabs.Town.NPC.Scripts
{
    public class Wheels : MonoBehaviour
    {
        [SerializeField] private Transform[] wheels;

        private void Update()
        {
            foreach (var wheel in wheels)
            {
                wheel.Rotate(Vector3.right, 300 * Time.deltaTime);
            }
        }
    }
}