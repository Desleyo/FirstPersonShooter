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
    public float reloadTime;

    [Header("Spray & Spread")]
    public float sprayPatternCorrection;
    public float maxSpread;

    [Header("Recoil")]
    public float camRecoilResetSpeed; //The speed at which the camera recoil will decay
    public float indexRecoilResetSpeed;
    public float maxRecoilY;

    [Header("Recoil pattern")]
    public Vector3[] recoilPattern;
}
