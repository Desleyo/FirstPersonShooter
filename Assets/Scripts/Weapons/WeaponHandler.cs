using System.Collections;
using UnityEngine;
using TMPro;

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Weapon weapon;
    [SerializeField] private GameObject decalPrefab;
    [SerializeField] private Transform decalParent;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI ammoText;

    private int currentAmmo;
    private int currentSprayIndex;
    private float lastTimeShot;

    private bool isReloading;

    private void Start()
    {
        currentAmmo = weapon.maxAmmo;

        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
    }

    private void Update()
    {
        Shoot();
        Reload();
    }

    private void Shoot()
    {
        //We'll return is we are not trying to shoot, don't have enough ammo, or are reloading
        if (!Input.GetButton("Fire1") || currentAmmo == 0 || isReloading)
            return;

        //We'll return if we are trying to shoot faster than our fireRate
        if (Time.time - lastTimeShot < 1 / weapon.fireRate)
            return;

        weapon.animator.SetTrigger("Shoot");

        Vector3 origin = playerCamera.transform.position;
        Vector3 direction = playerCamera.transform.TransformDirection(Vector3.forward);
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit))
        {
            Instantiate(decalPrefab, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal), decalParent);
        }

        currentAmmo--;
        currentSprayIndex++;
        lastTimeShot = Time.time;

        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
    }

    private void Reload()
    {
        //We'll return if we are not trying to reload and have more than zero ammo left
        if (!Input.GetButtonDown("Reload") && currentAmmo > 0)
            return;

        if (!isReloading)
        {
            StartCoroutine(StartReload());
        }
    }

    private IEnumerator StartReload()
    {
        isReloading = true;
        //weaponAnimator.SetTrigger("Reload");

        yield return new WaitForSeconds(weapon.reloadTime);

        currentAmmo = weapon.maxAmmo;
        ammoText.text = $"{currentAmmo} | {weapon.maxAmmo}";
        isReloading = false;
    }
}
