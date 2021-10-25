using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallHealth : MonoBehaviour
{
    [SerializeField] GameObject wall;
    [SerializeField] GameObject miniMapIcon;
    [SerializeField] GameObject brokenWall;
    [SerializeField] int maxHealth = 100;
    [SerializeField] float rebuildTime;
    [HideInInspector] public int health;
    GameObject newBrokenWall;

    private void Start()
    {
        health = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Transform[] decals = wall.GetComponentsInChildren<Transform>();
            foreach (Transform decal in decals)
            {
                if(decal.transform != wall.transform && decal.transform != miniMapIcon.transform)
                    Destroy(decal.gameObject);
            }

            wall.SetActive(false);
            newBrokenWall = Instantiate(brokenWall, transform);

            StartCoroutine(RebuildWall(rebuildTime));
        }
    }

    IEnumerator RebuildWall(float time)
    {
        yield return new WaitForSeconds(time);

        wall.SetActive(true);

        Destroy(newBrokenWall);

        health = maxHealth;
    }
}
