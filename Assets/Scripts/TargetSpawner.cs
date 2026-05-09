using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TargetGroup
    {
        public string colorName;
        public GameObject[] prefabs;
    }

    [Header("Target Variants")]
    public List<TargetGroup> targetGroups = new();

    [Header("Cluster Settings")]
    public int rocksPerCluster = 8;
    public float clusterRadius = 15f;
    public float clearRadius = 20f;

    [Header("Placement")]
    public LayerMask asteroidLayer = 6;
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    [Header("Spacing")]
    public float extraSpacing = 1f;

    public void SpawnTarget(List<GameManager.AsteroidData> asteroids)
    {
        if (asteroids == null || asteroids.Count == 0)
            return;

        List<GameManager.AsteroidData> validAsteroids = new();

        foreach (var asteroid in asteroids)
        {
            if (asteroid.rock.layer == 6)
            {
                validAsteroids.Add(asteroid);
            }
        }

        if (validAsteroids.Count == 0)
            return;

        GameManager.AsteroidData centerAsteroid =
            validAsteroids[Random.Range(0, validAsteroids.Count)];

        Vector3 clusterCenter = centerAsteroid.position;

        ClearArea(clusterCenter, asteroids);

        TargetGroup chosenGroup =
            targetGroups[Random.Range(0, targetGroups.Count)];

        // Local overlap tracking
        List<Vector3> placedPositions = new();
        List<float> placedRadii = new();

        for (int i = 0; i < rocksPerCluster; i++)
        {
            int attempts = 0;
            const int maxAttempts = 30;

            Vector3 spawnPos = Vector3.zero;
            float radius = 0f;
            float scale = 1f;
            GameObject prefab = null;

            do
            {
                Vector2 offset2D =
                    Random.insideUnitCircle * clusterRadius;

                spawnPos = clusterCenter +
                           new Vector3(offset2D.x, 0f, offset2D.y);

                prefab =
                    chosenGroup.prefabs[
                        Random.Range(0, chosenGroup.prefabs.Length)
                    ];

                scale = Random.Range(minScale, maxScale);

                radius =
                    GetPrefabRadius(prefab) * scale + extraSpacing;

                attempts++;

            } while (
                !IsValidPosition(
                    spawnPos,
                    radius,
                    placedPositions,
                    placedRadii
                )
                && attempts < maxAttempts
            );

            if (attempts >= maxAttempts)
                continue;

            Quaternion rot = Random.rotation;

            GameObject rock =
                Instantiate(prefab, spawnPos, rot);

            rock.transform.localScale *= scale;

            float actualRadius =
                GetPrefabRadius(prefab) * scale;

            var gm = GameManager.Instance;

            gm.RegisterAsteroid(
                spawnPos,
                actualRadius,
                rock
            );

            NavMeshObstacle obstacle = rock.AddComponent<NavMeshObstacle>();
            obstacle.carving = true;

            obstacle.shape = NavMeshObstacleShape.Capsule;

            obstacle.radius = radius / 2f;
            obstacle.height = radius;

            placedPositions.Add(spawnPos);
            placedRadii.Add(radius);

            rock.SetActive(false);
        }
    }

    bool IsValidPosition(
        Vector3 pos,
        float radius,
        List<Vector3> positions,
        List<float> radii
    )
    {
        for (int i = 0; i < positions.Count; i++)
        {
            float dist =
                Vector3.Distance(pos, positions[i]);

            if (dist < (radius + radii[i]))
                return false;
        }

        return true;
    }

    void ClearArea(
        Vector3 center,
        List<GameManager.AsteroidData> asteroids
    )
    {
        List<GameManager.AsteroidData> toRemove = new();

        foreach (var asteroid in asteroids)
        {
            if (asteroid.rock.layer != 6)
                continue;

            float dist =
                Vector3.Distance(center, asteroid.position);

            if (dist <= clearRadius)
            {
                asteroid.rock.SetActive(false);
                Destroy(asteroid.rock);

                toRemove.Add(asteroid);
            }
        }

        foreach (var asteroid in toRemove)
        {
            asteroids.Remove(asteroid);
        }
    }

    public float GetPrefabRadius(GameObject prefab)
    {
        Renderer rend =
            prefab.GetComponentInChildren<Renderer>();

        if (rend == null)
            return 0.5f;

        return rend.bounds.extents.magnitude;
    }
}