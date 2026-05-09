using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject[] enemyPrefabs;
    public int spawnCount = 4;

    [Header("Spawn Area")]
    public Vector2 xRange = new Vector2(-10f, 10f);
    public Vector2 zRange = new Vector2(-10f, 10f);

    [Header("Blocked Spawn Area")]
    public Vector2 blockedXRange = new Vector2(-20f, 20f);
    public Vector2 blockedZRange = new Vector2(-20f, 20f);

    public float yPosition = 0f;

    public void SpawnEnemies(GameManager.Area area)
    {
        for (int i = 0; i < spawnCount; i++)
        {
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            Vector3 spawnPosition;
            int safety = 0;

            do
            {
                float x = Random.Range(xRange.x, xRange.y);
                float z = Random.Range(zRange.x, zRange.y);

                spawnPosition = new Vector3(x, yPosition, z);

                safety++;
            }
            while (IsInsideBlockedArea(spawnPosition) && safety < 100);

            GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
            enemy.SetActive(false);
            area.enemies.Add(enemy);
        }
    }

    bool IsInsideBlockedArea(Vector3 pos)
    {
        return pos.x >= blockedXRange.x &&
               pos.x <= blockedXRange.y &&
               pos.z >= blockedZRange.x &&
               pos.z <= blockedZRange.y;
    }
}