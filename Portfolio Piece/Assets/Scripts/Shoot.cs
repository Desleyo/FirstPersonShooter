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
    int subtractFromDamage;
    [SerializeField] float autoFireRate;
    [SerializeField] float semiFireRate;

    [Header("Ammo & Reload variables")]
    [SerializeField] TextMeshProUGUI bulletCountText;
    [SerializeField] public int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    [HideInInspector] public bool isReloading;

    [Header("Recoil & Spread variables")]
    [SerializeField] TextMeshProUGUI spreadStatusText;
    [SerializeField] float recoilSpreadCorrection;
    [SerializeField] float[] spreadPatternX;
    [SerializeField] float[] spreadPatternY;
    [SerializeField] float normalSpread;
    [SerializeField] float movingSpread;
    [SerializeField] float jumpingSpread;
    bool spreadEnabled = true;
    Vector3 spreading;
    int patternIndex;

    [Space, SerializeField] float recoilValueX;
    [SerializeField] float recoilValueY;
    [SerializeField] float autoRecoilResetTime;
    [SerializeField] float semiRecoilResetTime;
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
    RaycastHit previousHit;

    private void Update()
    {
        CheckInput();

        if (currentBulletCount > 0 && !isReloading && nextTimeToShoot < Time.time)
        {
            if (isShooting || nextShot)
            {
                currentBulletCount--;
                patternIndex++;

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
                patternIndex = 0;
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

        //Add recoil to camera according to the fireMode
        float condition = spreadPatternX[patternIndex - 1];
        float recoilX = condition > 0 ? recoilValueX : condition < 0 ? -recoilValueX : 0;
        playerControls.AddRecoil(recoilX, recoilValueY);
        recoilResetTime = fullAuto ? autoRecoilResetTime : semiRecoilResetTime;

        //Add spread if enabled, but make sure first bullet accuracy is perserved
        spreading = new Vector3(0, 0, 0);
        if (spreadEnabled)
        {
            if(patternIndex != 1 || playerControls.isMoving || !playerControls.CanJump())
            {
                //Check how much spread is needed according to the movement of the player
                float spread = !playerControls.CanJump() ? jumpingSpread : playerControls.isMoving ? movingSpread : normalSpread;
                if (playerControls.isCrouching)
                    spread /= 2;

                float randomX;
                float randomY;
                if (!fullAuto)
                    randomX = randomY = Random.Range(-spread, spread);
                else
                {
                    if (spread == normalSpread || spread == normalSpread / 2)
                    {
                        randomX = spreadPatternX[patternIndex - 1] + Random.Range(-spread, spread);
                        randomY = spreadPatternY[patternIndex - 1];
                    }
                    else
                        randomX = randomY  = Random.Range(-spread, spread);
                }

                spreading = new Vector3(randomX / recoilSpreadCorrection, randomY / recoilSpreadCorrection, 0);
            }
        }

        ShootRaycast(cam.transform.position, spreading, 0, false, null);
    }

    void ShootRaycast(Vector3 point, Vector3 spread, int subtract, bool wallBanged, Collider wallBangedCollider)
    {
        subtractFromDamage += subtract;

        if (Physics.Raycast(point, cam.transform.TransformDirection(Vector3.forward + spread), out RaycastHit hit))
        {
            if (wallBanged)
                wallBangedCollider.enabled = true;

            string tag = hit.collider.tag;

            //Damage according to bodyShot values
            if (tag == "Body")
                hit.collider.GetComponent<EnemyHealth>().TakeDamage(bodyshotDamage - subtractFromDamage, false, wallBanged);
            //Damage according to headShot values
            else if (tag == "Head")
                hit.collider.GetComponentInParent<EnemyHealth>().TakeDamage(headshotDamage - subtractFromDamage, true, wallBanged);
            //Damage the wall
            else if (hit.collider.CompareTag("Wall"))
                hit.collider.GetComponentInParent<WallHealth>().TakeDamage(bodyshotDamage - subtractFromDamage);

            ImpactHole(tag, hit.point, hit);
            CheckForWallbang(hit);
        }
        else
        {
            if (wallBanged)
                wallBangedCollider.enabled = true;

            subtractFromDamage = 0;
        }
    }

    void CheckForWallbang(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        //Check if a new raycast needs to be fired from the hit point
        if (hitObject.layer == 7)
        {
            //Disable collider for just a frame to let the raycast through
            Collider hitCollider = hitObject.GetComponent<Collider>();
            hitCollider.enabled = false;

            //Shoot new raycast from the point you wallbanged
            Vector3 noSpread = new Vector3(0, 0, 0);
            ShootRaycast(hit.point, noSpread, wallbangDamageReducer, true, hitCollider);
        }
        else
            subtractFromDamage = 0;
    }

    void ImpactHole(string tag, Vector3 point, RaycastHit hit)
    {
        int health = tag == "Wall" ? hit.collider.GetComponentInParent<WallHealth>().health : 100;

        //Show impact hole
        if (tag != "Body" && tag != "Head" && health > 0)
        {
            Transform parent = tag == "Ground" || tag == "Untagged" ? bulletHitParent : hit.collider.gameObject.transform;
            var hitEffect = Instantiate(bulletHit, point, Quaternion.identity, parent);
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
