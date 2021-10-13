using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : MonoBehaviour
{
    [Header("Main references")]
    [SerializeField] PlayerController playerControls;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Animator reload;
    [SerializeField] GameObject armMesh;

    [Header("Flash & Bullet hole references")]
    [SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform bulletHitParent;

    [Header("Damage values")]
    [SerializeField] int bodyshotDamage;
    [SerializeField] int headshotDamage;
    [SerializeField] int wallbangDamageReducer;
    [SerializeField] float autoFireRate;
    [SerializeField] float semiFireRate;

    [Header("Ammo & Reload variables")]
    [SerializeField] TextMeshProUGUI bulletCountText;
    [SerializeField] public int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    [HideInInspector] public bool isReloading;

    [Header("Spray & Spread variables")]
    [SerializeField] Vector3[] sprayPattern;
    [SerializeField] float sprayCorrection;
    [SerializeField] bool spreadEnabled;
    [SerializeField] float spreadX;
    [SerializeField] float spreadY;
    int sprayPatternIndex;
    int revalueIndex;

    [Header("Recoil variables")]
    [Space, SerializeField] float recoilValueY;
    [SerializeField] float recoilResetAddTime;
    float recoilResetTime;

    [Header("FireMode variables")]
    [SerializeField] TextMeshProUGUI fireModeText;
    [SerializeField] float fireModeCooldown;
    bool canSwitchFireMode = true;

    //Some private variables
    [HideInInspector] public bool fullAuto = true;
    bool isShooting;
    bool nextShot;
    float nextTimeToShoot;

    private void Awake()
    {
        foreach (Vector3 spray in sprayPattern)
        {
            sprayPattern[revalueIndex] = new Vector3(sprayPattern[revalueIndex].x / sprayCorrection, sprayPattern[revalueIndex].y / sprayCorrection, 0);
            revalueIndex++;
        }
    }

    private void Update()
    {
        CheckInput();

        if (currentBulletCount > 0 && !isReloading && nextTimeToShoot < Time.time)
        {
            if (isShooting || nextShot)
            {
                currentBulletCount--;
                sprayPatternIndex++;

                ShootRaycast();
                InstantiateFlash();

                recoilResetTime = recoilResetAddTime;

                float fireRate = fullAuto ? autoFireRate : semiFireRate;
                nextTimeToShoot = Time.time + 1f / fireRate;
                nextShot = false;
            }
        }

        recoilResetTime -= Time.deltaTime;
        if (recoilResetTime <= 0)
        {
            sprayPatternIndex = 0;
            recoilResetTime = Mathf.Infinity;
        }
    }

    void CheckInput()
    {
        //Check if the player wants to fire a bullet
        isShooting = fullAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        //Check if the player wants to reload, or if the gun is empty
        if (Input.GetButtonDown("Reload") && !isReloading && nextTimeToShoot < Time.time && currentBulletCount != maxBulletCount || currentBulletCount == 0 && !isReloading && nextTimeToShoot < Time.time)
        {
            reload.gameObject.SetActive(true);
            armMesh.SetActive(false);

            reload.SetTrigger("Reload");
            StartCoroutine(Reload(reloadTime));
            isReloading = true;
        }

        //Display the bulletcount on screen
        if (!isReloading)
            bulletCountText.text = currentBulletCount + "|" + maxBulletCount;

        //Check if the player wants to switch firemodes
        if (Input.GetButton("FireMode") && canSwitchFireMode && !isShooting && !isReloading)
        {
            fullAuto = !fullAuto;
            fireModeText.text = fullAuto ? "V : Auto" : "V : Semi";
            canSwitchFireMode = false;
            StartCoroutine(FireModeCooldown(fireModeCooldown));
        }

        //check for input in between chambering rounds
        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;
    }

    public void ShootRaycast()
    {
        //Reset the animation for shooting
        animator.SetTrigger("Shoot");

        //Add recoil to camera
        playerControls.AddRecoil(sprayPattern[sprayPatternIndex - 1].x * sprayCorrection, recoilValueY);

        //Add spread if enabled, but make sure first bullet accuracy is perserved
        Vector3 spread;
        if (spreadEnabled && sprayPatternIndex != 1)
        {
            float randomX = Random.Range(-spreadX, spreadX);
            float randomY = Random.Range(0f, spreadY);

            spread = new Vector3(randomX / sprayCorrection, randomY / sprayCorrection, 0);
        }
        else
            spread = new Vector3(0, 0, 0);

        if (Physics.Raycast(cam.transform.position, cam.transform.TransformDirection(Vector3.forward + spread), out RaycastHit hit))
        {
            var hitEffect = Instantiate(bulletHit, hit.point, Quaternion.identity, bulletHitParent);
            hitEffect.transform.up = hit.normal;
            StartCoroutine(DestroyEffect(10f, hitEffect));
        }
    }

    void InstantiateFlash()
    {
        var effect = Instantiate(flash, muzzle.position, muzzle.rotation);
        effect.GetComponent<Transform>().SetParent(muzzle);
        StartCoroutine(DestroyEffect(.1f, effect));
    }

    IEnumerator DestroyEffect(float time, GameObject effect)
    {
        yield return new WaitForSeconds(time);

        Destroy(effect);

        StopCoroutine(nameof(DestroyEffect));
    }

    IEnumerator Reload(float time)
    {
        yield return new WaitForSeconds(time);

        nextShot = false;
        isReloading = false;

        animator.ResetTrigger("Reload");

        ResetAmmo();

        armMesh.SetActive(true);
        reload.gameObject.SetActive(false);

        StopCoroutine(nameof(Reload));
    }

    IEnumerator FireModeCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);

        canSwitchFireMode = true;
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }
}
