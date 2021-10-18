using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] int health = 100;
    bool gotHeadShot;
    bool gotWallBanged;

    public void TakeDamage(int damage, bool headShot, bool wallBanged)
    {
        health -= damage;
        gotHeadShot = headShot;
        gotWallBanged = wallBanged;

        if (health <= 0)
        {
            EnemySpawner.enemySpawner.SpawnEnemy(transform);
            UpdateKillFeed();
        }
    }

    void UpdateKillFeed()
    {
        //Update the killfeed using symbols for headshots & wallbangs

        Destroy(gameObject);
    }
}
