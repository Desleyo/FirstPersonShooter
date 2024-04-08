using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    public string enemyName; //We'll access this in the enemySpawner

    [Space]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private int health = 100;
    
    private bool hasDied = false;

    private void Start()
    {
        healthText.text = health.ToString();
    }

    public void TakeDamage(int damage, bool headshot = false, bool wallbanged = false)
    {
        health -= damage;

        healthText.text = health.ToString();

        if (health <= 0 && !hasDied)
        {
            hasDied = true;

            Killfeed.instance.UpdateKillFeed(enemyName, headshot, wallbanged);

            EnemySpawner.instance.SpawnEnemy(transform, enemyName);

            Destroy(gameObject);
        }
    }
}
