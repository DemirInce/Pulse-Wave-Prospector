using UnityEngine;
using System.Collections.Generic;

public class RockSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform plane;              
    public GameObject[] rockPrefabs;     

    [Header("Spawn Settings")]
    public int rockCount = 50;

    public int Layer = 0;
    public float heightVariation = 0f;

    [Header("Randomization")]
    public Vector2 scaleRange = new Vector2(0.5f, 2f);

    [Header("Spacing")]
    public float extraSpacing = 0.2f;

    [Header("Blocked Spawn Area")]
    public Vector2 blockedXRange = new Vector2(-15f, 15f);
    public Vector2 blockedZRange = new Vector2(-15f, 15f);

    public List<Vector3> placedPositions = new List<Vector3>();
    public List<float> placedRadii = new List<float>();

    public void SpawnRocks(GameManager.Area area)
    {
        if (plane == null || rockPrefabs.Length == 0)
        {
            Debug.LogWarning("Missing plane or rock prefabs!");
            return;
        }

        for (int i = 0; i < rockCount; i++)
        {
            GameObject prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];

            int attempts = 0;
            const int maxAttempts = 30;

            Vector3 spawnPos;
            float scale;
            float radius;

            do
            {
                spawnPos = GetRandomPointOnPlane();
                scale = Random.Range(scaleRange.x, scaleRange.y);
                radius = GetPrefabRadius(prefab) * scale + extraSpacing;

                spawnPos.y += Random.Range(-heightVariation, heightVariation);

                attempts++;
            }
            while (!IsValidPosition(spawnPos, radius) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
                continue;

            Quaternion rotation = Random.rotation;

            GameObject rock = Instantiate(prefab, spawnPos, rotation);
            rock.transform.localScale *= scale;
            rock.layer = Layer;

            if (area != null)
            {
                rock.SetActive(false);
                area.asteroids.Add(new GameManager.AsteroidData(spawnPos, radius, rock));
            }

            placedPositions.Add(spawnPos);
            placedRadii.Add(radius);
        }
    }

    public bool IsValidPosition(Vector3 pos, float radius)
    {
        for (int i = 0; i < placedPositions.Count; i++)
        {
            float dist = Vector3.Distance(pos, placedPositions[i]);
            if (dist < (radius + placedRadii[i]))
                return false;
        }
        return !IsInsideBlockedArea(pos);
    }

    public float GetPrefabRadius(GameObject prefab)
    {
        Renderer rend = prefab.GetComponentInChildren<Renderer>();
        if (rend == null)
            return 0.5f;

        return rend.bounds.extents.magnitude;
    }

    public Vector3 GetRandomPointOnPlane()
    {
        float width = plane.localScale.x * 10f;
        float length = plane.localScale.z * 10f;

        float x = Random.Range(-width / 2f, width / 2f);
        float z = Random.Range(-length / 2f, length / 2f);

        Vector3 center = plane.position;

        return new Vector3(center.x + x, center.y, center.z + z);
    }

    bool IsInsideBlockedArea(Vector3 pos)
    {
        return pos.x >= blockedXRange.x &&
               pos.x <= blockedXRange.y &&
               pos.z >= blockedZRange.x &&
               pos.z <= blockedZRange.y;
    }
}