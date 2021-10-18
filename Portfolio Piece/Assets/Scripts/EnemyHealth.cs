using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] int health = 100;
    bool gotHeadShot;
    bool gotWallBanged;

    private void Start()
    {
        healthText.text = health.ToString();
    }

    public void TakeDamage(int damage, bool headShot, bool wallBanged)
    {
        health -= damage;
        gotHeadShot = headShot;
        gotWallBanged = wallBanged;

        healthText.text = health.ToString();

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
