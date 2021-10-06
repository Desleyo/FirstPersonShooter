using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Shoot : MonoBehaviour
{
    //References
    [SerializeField] Camera cam;
    [SerializeField] Animator animator;
    [SerializeField] Animator reload;
    [SerializeField] GameObject armMesh;

    [Space, SerializeField] GameObject flash;
    [SerializeField] GameObject bulletHit;
    [SerializeField] Transform muzzle;

    //Damage values & fire rate
    [Space, SerializeField] int bodyshotDamage;
    [SerializeField] int headshotDamage;
    [SerializeField] float fireRate;

    //Bullet & reload values
    [Space, SerializeField] int currentBulletCount;
    [SerializeField] int maxBulletCount;
    [SerializeField] float reloadTime;
    bool reloading;

    [SerializeField] TextMeshProUGUI bulletCountText;
    
    //Spray pattern & recoil values
    [Space, SerializeField] Vector3[] sprayPattern;
    [SerializeField] int sprayPatternIndex;
    [SerializeField] float recoilResetAddTime;
    float recoilResetTime;
    int count;

    float nextTimeToShoot;
    bool isShooting;
    bool nextShot;
    bool fullAuto = true;

    private void Update()
    {
        CheckFireModeInput();
        CheckReloadInput();

        if (currentBulletCount > 0 && !reloading && Time.time >= nextTimeToShoot)
        {
            if (isShooting || nextShot)
            {
                currentBulletCount--;
                sprayPatternIndex++;
                ShootRaycast();
                InstantiateFlash();
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
            if (!fullAuto)
                isShooting = Input.GetButtonDown("Fire1") && nextTimeToShoot < Time.time;
            else
                isShooting = Input.GetButton("Fire1") && nextTimeToShoot < Time.time;

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
        if(cam.transform.forward.z >= 0 && cam.transform.forward.z <= 1)
            dir = cam.transform.forward + sprayPattern[sprayPatternIndex - 1];
        else
            dir = cam.transform.forward + new Vector3(-sprayPattern[sprayPatternIndex -1].x, sprayPattern[sprayPatternIndex -1].y, 0);

        Vector3 ray = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.5f));
        RaycastHit hit;
        if(Physics.Raycast(ray, dir, out hit))
        {
            var hitEffect = Instantiate(bulletHit, hit.point, Quaternion.identity);
            StartCoroutine(DestroyEffect(.25f, hitEffect));   
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
