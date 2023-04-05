using UnityEngine;

//This script forces the GameObject this scripts is attached to, to always look at the main camera in the scene
public class LookatPlayer : MonoBehaviour
{
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void LateUpdate()
    {
        transform.LookAt(cameraTransform);
    }
}
