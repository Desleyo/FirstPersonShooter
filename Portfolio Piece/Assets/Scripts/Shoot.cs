using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : MonoBehaviour
{
    //References
    [SerializeField] PlayerController playerControls;
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Animator reload;
    [SerializeField] GameObject armMesh;

    [Space, SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;
    [SerializeField] Transform bulletHitParent;

    //Damage values & fire rate
    [Space, SerializeField] int bodyshotDamage;
    [SerializeField] int headshotDamage;
    [SerializeField] int wallbangDamageReducer;
    [SerializeField] float fireRate;

    //Bullet & reload variables
    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    public bool reloading;

    [SerializeField] TextMeshProUGUI bulletCountText;
    
    //Spray pattern variables
    [Space, SerializeField] Vector3[] sprayPattern;
    [SerializeField] float crouchSpray;
    [SerializeField] float sprayDivider;
    [SerializeField] bool spreadEnabled;
    public int sprayPatternIndex;
    int revalueIndex;

    //Recoil variables
    [Space, SerializeField] float recoilValueY;
    [SerializeField] float recoilResetAddTime;
    float recoilResetTime;

    float nextTimeToShoot;
    public bool isShooting;
    public bool nextShot;
    bool fullAuto = true;

    private void Start()
    {
        foreach (Vector3 spray in sprayPattern)
        {
            sprayPattern[revalueIndex] = new Vector3(sprayPattern[revalueIndex].x / sprayDivider, sprayPattern[revalueIndex].y / sprayDivider, 0);
            revalueIndex++;
        }
    }

    private void Update()
    {
        CheckFireModeInput();
        CheckReloadInput();

        if (currentBulletCount > 0 && !reloading && nextTimeToShoot < Time.time)
        {
            if (isShooting || nextShot)
            {
                currentBulletCount--;
                sprayPatternIndex++;

                ShootRaycast();
                InstantiateFlash();
                playerControls.AddRecoil(sprayPattern[sprayPatternIndex - 1].x * sprayDivider, recoilValueY);

                recoilResetTime = recoilResetAddTime;
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

    void CheckFireModeInput()
    {
        //switch between fire modes
        if (Input.GetButtonDown("FireMode")) 
            fullAuto = !fullAuto;  

        //check input of mouse
        isShooting = fullAuto ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        //check for input in between chambering rounds
        if (nextTimeToShoot > Time.time && Input.GetButtonDown("Fire1") && !isShooting)
            nextShot = true;
        else if (nextTimeToShoot > Time.time && Input.GetButtonUp("Fire1"))
            nextShot = false;
    }

    void CheckReloadInput()
    {
        //Check if the player wants to reload, or if the gun is empty
        if (Input.GetButtonDown("Reload") && !reloading && nextTimeToShoot < Time.time && currentBulletCount != maxBulletCount || currentBulletCount == 0 && !reloading && nextTimeToShoot < Time.time)
        {
            reload.gameObject.SetActive(true);
            armMesh.SetActive(false);

            reload.SetTrigger("Reload");
            StartCoroutine(Reload(reloadTime));
            reloading = true;
        }
        if (!reloading)
            bulletCountText.text = currentBulletCount + "|" + maxBulletCount;
    }

    public void ShootRaycast()
    {
        animator.SetTrigger("Shoot");

        //adjust spray pattern according to transform.forward
        Vector3 dir;
        if(cam.transform.forward.z >= 0)
            dir = cam.transform.forward + sprayPattern[sprayPatternIndex - 1];
        else
            dir = cam.transform.forward + new Vector3(-sprayPattern[sprayPatternIndex -1].x, sprayPattern[sprayPatternIndex -1].y, 0);

        //Lower the spray if crouching
        if(playerControls.isCrouching)
            dir = new Vector3(dir.x / crouchSpray, dir.y / crouchSpray, dir.z);

        //Add spread if enabled && perserve first bullet accuracy
        if(spreadEnabled && sprayPatternIndex != 1)
        {
            float randomX = Random.Range(-.25f, .25f);
            float randomY = Random.Range(0f, .25f);
            dir = new Vector3(dir.x + randomX / sprayDivider, dir.y + randomY / sprayDivider, dir.z);
        }

        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
        if(Physics.Raycast(ray, dir, out RaycastHit hit))
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
        reloading = false;

        animator.ResetTrigger("Reload");

        ResetAmmo();

        armMesh.SetActive(true);
        reload.gameObject.SetActive(false);

        StopCoroutine(nameof(Reload));
    }

    public void ResetAmmo()
    {
        currentBulletCount = maxBulletCount;
    }
}
