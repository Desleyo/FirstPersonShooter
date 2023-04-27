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
    public float sprayPatternCorrection; //Divide the recoil pattern with this correction to create a spray pattern
    public float maxSpread;

    [Header("Recoil")]
    public float camRecoilResetSpeed; //The speed at which the camera recoil will decay
    public float indexRecoilResetSpeed; //The speed at which the spray pattern index will decay
    public float maxRecoilY;

    [Header("Recoil pattern")]
    public Vector3[] recoilPattern;
}
