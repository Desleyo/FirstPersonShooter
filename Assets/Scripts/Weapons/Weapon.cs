using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform muzzleFlashPoint;

    [Header("General")]
    public string weaponName;
    public int maxAmmo;
    public float fireRate;

    [Header("Recoil & Spread")]
    public float recoilResetTime; //The time it will take to reset the recoil pattern
    public float recoilResetSpeed; //The speed at which the camera recoil will decline
    public float reloadTime;
    public float maxSpread;

    [Header("Recoil pattern")]
    public Vector3[] recoilPattern;
}
