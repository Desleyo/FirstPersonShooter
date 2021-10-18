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
    [SerializeField] int subtractFromDamage;
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
    int sprayPatternIndex;
    int revalueIndex;

    [SerializeField] TextMeshProUGUI spreadStatusText;
    [SerializeField] float spreadX;
    [SerializeField] float spreadY;
    bool spreadEnabled = true;

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

                SetupRaycast();

                float fireRate = fullAuto ? autoFireRate : semiFireRate;
                nextTimeToShoot = Time.time + 1f / fireRate;
                nextShot = false;
            }
        }

        if(recoilResetTime > 0)
        {
            recoilResetTime -= Time.deltaTime;
            if (recoilResetTime <= 0)
            {
                sprayPatternIndex = 0;
                recoilResetTime = Mathf.Infinity;
            }
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

        //Check if the player wants to (De)Activate the spread
        if (Input.GetButtonDown("SetSpread"))
        {
            spreadEnabled = !spreadEnabled;
            spreadStatusText.text = spreadEnabled ? "B : Spread on" : "B : Spread off";
            spreadStatusText.color = spreadEnabled ? Color.green : Color.red;
        }

        //check for input in between chambering rounds
        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1"))
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;
    }

    public void SetupRaycast()
    {
        animator.SetTrigger("Shoot");

        InstantiateFlash();

        //Add recoil to camera
        playerControls.AddRecoil(sprayPattern[sprayPatternIndex - 1].x * sprayCorrection, recoilValueY);
        recoilResetTime = recoilResetAddTime;

        //Add spread if enabled, but make sure first bullet accuracy is perserved
        Vector3 spreading;
        if (spreadEnabled && sprayPatternIndex != 1)
        {
            float randomX = Random.Range(-spreadX, spreadX);
            float randomY = Random.Range(0f, spreadY);

            spreading = new Vector3(randomX / sprayCorrection, randomY / sprayCorrection, 0);
        }
        else
            spreading = new Vector3(0, 0, 0);

        ShootRaycast(cam.transform.position, spreading, 0, false);
    }

    void ShootRaycast(Vector3 point, Vector3 spread, int subtract, bool wallBanged)
    {
        subtractFromDamage += subtract;

        if (Physics.Raycast(point, cam.transform.TransformDirection(Vector3.forward + spread), out RaycastHit hit))
        {
            //Damage according to bodyShot values
            if (hit.collider.CompareTag("Body"))
                hit.collider.GetComponent<EnemyHealth>().TakeDamage(bodyshotDamage - subtractFromDamage, false, wallBanged);
            //Damage according to headShot values
            else if (hit.collider.CompareTag("Head"))
                hit.collider.GetComponentInParent<EnemyHealth>().TakeDamage(headshotDamage - subtractFromDamage, true, wallBanged);

            CheckForWallbang(hit, spread);
        }
    }

    void CheckForWallbang(RaycastHit rayHit, Vector3 spread)
    {
        //Check if a new raycast needs to be fired from the hit point
        if (rayHit.collider.gameObject.layer == 7)
        {
            //Shoot new raycast from the point you wallbanged + add 1 on the forward axis so u dont wallbang the same object
            ShootRaycast(rayHit.point + cam.transform.TransformDirection(new Vector3(0, 0, 1)), spread, wallbangDamageReducer, true);
        }
        else
        {
            //Show impact hole
            var hitEffect = Instantiate(bulletHit, rayHit.point, Quaternion.identity, bulletHitParent);
            hitEffect.transform.up = rayHit.normal;
            StartCoroutine(DestroyEffect(10f, hitEffect));

            subtractFromDamage = 0;
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
