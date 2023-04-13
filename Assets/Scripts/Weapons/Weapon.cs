using UnityEngine;

public class Weapon : MonoBehaviour
{
    public Animator animator;
    public string weaponName;
    public int maxAmmo;
    public float fireRate;
    public float reloadTime;
    public Vector3[] sprayPattern;
}
