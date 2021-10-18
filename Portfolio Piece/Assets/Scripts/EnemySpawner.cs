using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner enemySpawner;
    [SerializeField] GameObject enemy;
    [SerializeField] float timeToSpawn = 3f;

    private void Awake()
    {
        enemySpawner = this;
    }

    public void SpawnEnemy(Transform point)
    {
        Vector3 enemyPos = point.position;
        StartCoroutine(SpawnNewEnemy(enemyPos, timeToSpawn));
    }

    IEnumerator SpawnNewEnemy(Vector3 pos, float time)
    {
        yield return new WaitForSeconds(time);

        GameObject newEmeny = Instantiate(enemy);
        newEmeny.transform.position = pos;
    }
}
