using UnityEngine;

public class BulletHole : MonoBehaviour
{
    [SerializeField] private float destroyTime;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
