using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] private float destroyTime;

    private void Start()
    {
        Destroy(gameObject, destroyTime);
    }
}
