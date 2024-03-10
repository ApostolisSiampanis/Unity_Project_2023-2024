using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraZoneSwitcher : MonoBehaviour
{
    public string triggerTag;

    public Camera primaryCamera;

    public Camera[] cameras;

    private void Start()
    {
        SwitchToCamera(primaryCamera);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            Camera targetCamera = other.GetComponentInChildren<Camera>();
            SwitchToCamera(targetCamera);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(triggerTag))
        {
            SwitchToCamera(primaryCamera);
        }
    }

    private void SwitchToCamera(Camera targetCamera)
    {
        foreach (Camera camera in cameras)
        {
            if (camera == targetCamera)
            {
                camera.enabled = true;
            }
            else
            {
                camera.enabled = false;
            }
        }
    }
}