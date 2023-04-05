using System.Collections;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner instance;

    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float timeToSpawn = 3f;

    private void Awake()
    {
        instance = this;
    }

    //Call this function to spawn a new enemy on the same position the previous enemy died
    public void SpawnEnemy(Transform enemyTransform, string enemyName)
    {
        Vector3 enemyPos = enemyTransform.position;
        StartCoroutine(SpawnNewEnemy(enemyPos, enemyName, timeToSpawn));
    }

    //Call this enumerator to spawn the new enemy on the given spawn position, after a given amount of seconds
    private IEnumerator SpawnNewEnemy(Vector3 spawnPos, string enemyName, float spawnTime)
    {
        yield return new WaitForSeconds(spawnTime);

        GameObject newEmeny = Instantiate(enemyPrefab);
        newEmeny.transform.position = spawnPos;
        newEmeny.GetComponent<EnemyHealth>().enemyName = enemyName;
    }
}
