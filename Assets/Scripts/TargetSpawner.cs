using System.Collections.Generic;
using UnityEngine;

public class TargetSpawner : MonoBehaviour
{
    [System.Serializable]
    public class TargetGroup
    {
        public string colorName;
        public GameObject[] prefabs;
        public GameObject targetNavMeshObstaclePrefab;
        public Material Skybox;
    }

    [Header("Target Variants")]
    public List<TargetGroup> targetGroups = new();

    [Header("Cluster Settings")]
    public int rocksPerCluster = 8;
    public float clusterRadius = 15f;
    public float clearRadius = 20f;

    [Header("Placement")]
    public LayerMask asteroidLayer = 6;
    public int layer; 
    public float minScale = 0.8f;
    public float maxScale = 1.5f;

    [Header("Spacing")]
    public float extraSpacing = 1f;

    public class Return
    {
        public GameObject target;
        public Vector2 coords;
        public int num;
    }

    public Return SpawnTarget(List<GameManager.AsteroidData> asteroids, GameManager.Area area)
    {
        var gm = GameManager.Instance;
        if (asteroids == null || asteroids.Count == 0)
            return null;

        List<GameManager.AsteroidData> validAsteroids = new();

        foreach (var asteroid in asteroids)
        {
            if (asteroid.rock.layer == 6)
            {
                validAsteroids.Add(asteroid);
            }
        }

        if (validAsteroids.Count == 0)
            return null;

        List<GameManager.AsteroidData> boundedAsteroids = new();
        foreach (var asteroid in validAsteroids)
        {
            Vector3 pos = asteroid.position;

            if (
                pos.x >= -100f && pos.x <= 100f &&
                pos.z >= -100f && pos.z <= 100f
            )
            {
                boundedAsteroids.Add(asteroid);
            }
        }

        if (boundedAsteroids.Count == 0)
            return null;

        GameManager.AsteroidData centerAsteroid = boundedAsteroids[Random.Range(0, boundedAsteroids.Count)];
        Vector3 clusterCenter = centerAsteroid.position;

        ClearArea(clusterCenter, asteroids);

        TargetGroup chosenGroup = targetGroups[Random.Range(0, targetGroups.Count)];

        RenderSettings.skybox = chosenGroup.Skybox;
        DynamicGI.UpdateEnvironment();

        GameObject centerObstacle = Instantiate(chosenGroup.targetNavMeshObstaclePrefab, clusterCenter, Quaternion.identity);
        area.asteroids.Add(new GameManager.AsteroidData(clusterCenter, 0f, centerObstacle));
        centerObstacle.SetActive(false);
        
        List<Vector3> placedPositions = new();
        List<float> placedRadii = new();

        int count = 0;
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
                Vector2 offset2D = Random.insideUnitCircle * clusterRadius;

                spawnPos = clusterCenter + new Vector3(offset2D.x, 0f, offset2D.y);
                prefab = chosenGroup.prefabs[Random.Range(0, chosenGroup.prefabs.Length)];
                scale = Random.Range(minScale, maxScale);
                radius = GetPrefabRadius(prefab) * scale + extraSpacing;

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

            GameObject rock = Instantiate(prefab, spawnPos, rot);
            count += 1;

            rock.transform.localScale *= scale;
            rock.layer = layer;

            float actualRadius = GetPrefabRadius(prefab) * scale;

            area.asteroids.Add(new GameManager.AsteroidData(spawnPos, actualRadius, rock));

            placedPositions.Add(spawnPos);
            placedRadii.Add(radius);

            rock.SetActive(false);
        }

        Return r = new Return();
        r.target = centerObstacle;
        r.coords = clusterCenter;
        r.num = count;
        return r;
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
            float dist = Vector3.Distance(pos, positions[i]);
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