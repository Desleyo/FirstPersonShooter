using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour
{
    public string enemyName;

    [Space, SerializeField] TextMeshProUGUI healthText;
    [SerializeField] int health = 100;
    bool died;

    private void Start()
    {
        healthText.text = health.ToString();
        died = false;
    }

    public void TakeDamage(int damage, bool headShot, bool wallBanged)
    {
        health -= damage;

        healthText.text = health.ToString();

        if (health <= 0 && !died)
        {
            died = true;

            KillFeed.killFeed.UpdateKillFeed(enemyName, headShot, wallBanged);
            EnemySpawner.enemySpawner.SpawnEnemy(transform, enemyName);

            Destroy(gameObject);
        }
    }
}
