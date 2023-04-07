using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform cameraPos;

    private void Update()
    {
        transform.position = cameraPos.position;
    }
}
