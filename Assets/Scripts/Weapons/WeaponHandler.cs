using System.Collections;
using UnityEngine;
using TMPro;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private CameraMovement cameraMovement;
    [SerializeField] private Weapon weapon;

    [Header("Decals")]
    [SerializeField] private GameObject muzzleFlashPrefab;
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private Transform decalParent;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private int currentAmmo;
    private int currentRecoilIndex;
    private float lastTimeShot;
    private float recoilResetTimeLeft;

    private bool wantsToShoot;
    private bool wantsToReload;
    private bool isShooting;
    private bool isReloading;

    private void Start()
    {
        currentAmmo = weapon.maxAmmo;

        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
    }

    private void Update()
    {
        GetInputs();
        Shoot();
        Reload();
    }

    private void GetInputs()
    {
        wantsToShoot = Input.GetButton("Fire1");
        wantsToReload = Input.GetButtonDown("Reload");
    }

    private void Shoot()
    {
        //We'll return is we are not trying to shoot, don't have enough ammo, or are reloading
        if (!wantsToShoot || currentAmmo == 0 || isReloading)
        {
            isShooting = false;

            HandleRecoil();

            return;
        }

        //We'll return if we are trying to shoot faster than our fireRate
        if (Time.time - lastTimeShot < 1 / weapon.fireRate)
            return;

        isShooting = true;
        weapon.animator.SetTrigger("Shoot");

        recoilResetTimeLeft = weapon.recoilResetTime;

        //Handle the recoil of the gun
        Vector3 recoil = weapon.recoilPattern[currentRecoilIndex];
        cameraMovement.AddRecoil(cameraMovement.currentRecoil + recoil);

        //Create a randomSpread Vector3 with random values for X and Y
        float randomSpreadX = Random.Range(-weapon.maxSpread, weapon.maxSpread);
        float randomSpreadY = Random.Range(-weapon.maxSpread, weapon.maxSpread);
        Vector3 randomSpread = new Vector3(randomSpreadX, randomSpreadY, 0);

        //Setup the raycast
        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward + randomSpread);
        if (Physics.Raycast(origin, direction, out RaycastHit hit))
        {
            //Do something on hit

            //Create bullet hole
            GameObject decal = Instantiate(decalPrefab, hit.point, Quaternion.identity, decalParent);
            decal.transform.up = hit.normal;
        }

        //Create muzzle flash
        Instantiate(muzzleFlashPrefab, weapon.muzzleFlashPoint);

        currentAmmo--;
        currentRecoilIndex++;
        lastTimeShot = Time.time;
        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
    }

    private void HandleRecoil()
    {
        if (recoilResetTimeLeft > 0)
        {
            recoilResetTimeLeft -= Time.deltaTime;
        }
        else if (currentRecoilIndex != 0)
        {
            currentRecoilIndex = 0;
        }

        if (cameraMovement.currentRecoil != Vector3.zero)
        {
            //Slowly reset the recoil of the gun
            Vector3 resetRecoil = Vector3.Lerp(cameraMovement.currentRecoil, Vector3.zero, weapon.recoilResetSpeed * Time.deltaTime);
            cameraMovement.AddRecoil(resetRecoil);
        }
    }

    private void Reload()
    {
        //We'll return if we are not trying to reload and have more than zero ammo left
        if (!wantsToReload && currentAmmo > 0)
            return;

        if (!isReloading && !isShooting && currentAmmo < weapon.maxAmmo)
        {
            StartCoroutine(StartReload());
        }
    }

    private IEnumerator StartReload()
    {
        isReloading = true;
        weapon.animator.SetTrigger("Reload");

        yield return new WaitForSeconds(weapon.reloadTime);

        currentAmmo = weapon.maxAmmo;
        currentRecoilIndex = 0;
        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
        isReloading = false;
    }
}
