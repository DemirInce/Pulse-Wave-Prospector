using UnityEngine;
using UnityEngine.AI;
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

    private List<Vector3> placedPositions = new List<Vector3>();
    private List<float> placedRadii = new List<float>();

    void Start()
    {
        SpawnRocks();
    }

    void SpawnRocks()
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

            if (Layer == 6)
            {
                NavMeshObstacle obstacle = rock.AddComponent<NavMeshObstacle>();
                obstacle.carving = true;

                obstacle.shape = NavMeshObstacleShape.Capsule;

                obstacle.radius = radius / 2f;
                obstacle.height = radius;
            }

            placedPositions.Add(spawnPos);
            placedRadii.Add(radius);
        }
    }

    bool IsValidPosition(Vector3 pos, float radius)
    {
        for (int i = 0; i < placedPositions.Count; i++)
        {
            float dist = Vector3.Distance(pos, placedPositions[i]);
            if (dist < (radius + placedRadii[i]))
                return false;
        }
        return true;
    }

    float GetPrefabRadius(GameObject prefab)
    {
        Renderer rend = prefab.GetComponentInChildren<Renderer>();
        if (rend == null)
            return 0.5f;

        return rend.bounds.extents.magnitude;
    }

    Vector3 GetRandomPointOnPlane()
    {
        float width = plane.localScale.x * 10f;
        float length = plane.localScale.z * 10f;

        float x = Random.Range(-width / 2f, width / 2f);
        float z = Random.Range(-length / 2f, length / 2f);

        Vector3 center = plane.position;

        return new Vector3(center.x + x, center.y, center.z + z);
    }
}