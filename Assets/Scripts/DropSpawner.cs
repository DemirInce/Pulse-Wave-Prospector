using System.Collections.Generic;
using UnityEngine;

public class DropSpawner : MonoBehaviour
{
    public List<GameObject> Pickups;
    public int DropDivider = 100;
    public float DropRadius = 4f;

    public void SpawnDrops(float maxHealth)
    {
        int amount = Mathf.CeilToInt(maxHealth / DropDivider);

        for (int i = 0; i < amount; i++)
        {
            GameObject prefab = Pickups[Random.Range(0, Pickups.Count)];

            Vector2 offset = Random.insideUnitCircle * DropRadius;
            Vector3 spawnPos = transform.position + new Vector3(offset.x, 0f, offset.y);

            Instantiate(prefab, spawnPos, Quaternion.identity);
        }
    }
}