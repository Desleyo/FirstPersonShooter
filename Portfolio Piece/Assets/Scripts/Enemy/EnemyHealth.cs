using UnityEngine;
using TMPro;

public class EnemyHealth : MonoBehaviour, IDamagable
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

    //Call this function to deal damage
    public void TakeDamage(int damage, bool headShot = false, bool wallBanged = false)
    {
        health -= damage;

        healthText.text = health.ToString();

        if (health <= 0 && !hasDied)
        {
            hasDied = true;

            KillFeed.instance.UpdateKillFeed(enemyName, headShot, wallBanged);

            EnemySpawner.instance.SpawnEnemy(transform, enemyName);

            Destroy(gameObject);
        }
    }
}
