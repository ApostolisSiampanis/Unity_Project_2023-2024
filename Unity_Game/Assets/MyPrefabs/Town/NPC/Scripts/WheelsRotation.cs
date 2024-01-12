using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheels : MonoBehaviour
{
    [SerializeField] private Transform[] wheels;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // warning This is a hack.  It should be done with an animation.
        foreach (var wheel in wheels)
        {
            wheel.Rotate(Vector3.right, 1000 * Time.deltaTime);
        }
    }
}
