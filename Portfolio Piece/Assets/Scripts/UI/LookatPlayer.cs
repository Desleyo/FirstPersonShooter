using UnityEngine;

public class LookatPlayer : MonoBehaviour
{
    Transform cam;

    void Start()
    {
        cam = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Update()
    {
        transform.LookAt(cam);
    }
}
