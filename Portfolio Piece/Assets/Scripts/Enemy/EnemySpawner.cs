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

    public void SpawnEnemy(Transform point, string name)
    {
        Vector3 enemyPos = point.position;
        StartCoroutine(SpawnNewEnemy(enemyPos, name, timeToSpawn));
    }

    IEnumerator SpawnNewEnemy(Vector3 pos, string name, float time)
    {
        yield return new WaitForSeconds(time);

        GameObject newEmeny = Instantiate(enemy);
        newEmeny.transform.position = pos;
        newEmeny.GetComponent<EnemyHealth>().enemyName = name;
    }
}
