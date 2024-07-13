using System.Collections;
using UnityEngine;

public class WallHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private GameObject wall;
    [SerializeField] private GameObject brokenWall;

    [Space]
    [SerializeField] private GameObject miniMapIcon;

    [Space]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private float rebuildTime;

    [HideInInspector] public int health;

    private bool isBroken = false;

    private void Start()
    {
        health = maxHealth;
    }

    //Call this function to deal damage
    public void TakeDamage(int damage, bool headshot = false, bool wallbanged = false)
    {
        //We don't want to take damage when the wall is already broken
        if(isBroken) 
            return;

        health -= damage;

        if (health <= 0)
        {
            Invoke(nameof(DestroyWall), .01f);
            isBroken = true;
        }
    }

    //Call this function to destroy the wall
    private void DestroyWall()
    {
        //We'll get every transform component in the children of this wall
        Transform[] decals = wall.GetComponentsInChildren<Transform>();
        foreach (Transform decal in decals)
        {
            //Make sure we don't destroy the wall or the miniMapIcon
            if (decal.gameObject != wall && decal.gameObject != miniMapIcon)
            {
                Destroy(decal.gameObject);
            }
        }

        //We'll disable the wall instead of destroying it, so we can enable it after the rebuildTime
        wall.SetActive(false);
        
        GameObject newBrokenWall = Instantiate(brokenWall, transform);

        StartCoroutine(RebuildWall(newBrokenWall, rebuildTime));
    }

    //Call this function to destroy the broken wall and enable the normal wall after a given amount of time
    private IEnumerator RebuildWall(GameObject newBrokenWall, float time)
    {
        yield return new WaitForSeconds(time);

        Destroy(newBrokenWall);

        wall.SetActive(true);

        health = maxHealth;
        isBroken = false;
    }
}
