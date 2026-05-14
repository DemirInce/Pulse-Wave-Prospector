using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform plane;              
    public GameObject[] objectPrefabs;     

    [Header("Spawn Settings")]
    public int Count = 50;

    [Header("Spacing")]
    public float minimumObjectDistance = 80f;

    private RockSpawner rockSpawner;

    void Awake()
    {
        rockSpawner = GetComponent<RockSpawner>();
    }

    public void SpawnObjects(GameManager.Area area)
    {
        if (plane == null || objectPrefabs.Length == 0)
        {
            Debug.LogWarning("Missing plane or prefabs!");
            return;
        }

        for (int i = 0; i < Count; i++)
        {
            GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Length)];

            int attempts = 0;
            const int maxAttempts = 30;

            Vector3 spawnPos;
            float radius;

            do
            {
                spawnPos = rockSpawner.GetRandomPointOnPlane();
                radius = rockSpawner.GetPrefabRadius(prefab) + 10;

                attempts++;
            }
            while (!IsValidPosition(area, spawnPos, radius) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
                continue;

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

            obj.SetActive(false);
            area.objects.Add(obj);
        }
    }

    bool IsValidPosition(GameManager.Area area, Vector3 position, float radius)
    {
        foreach (var asteroid in area.asteroids)
        {
            float dist = Vector3.Distance(position, asteroid.position);

            if (dist < radius + asteroid.radius)
                return false;
        }

        foreach (GameObject obj in area.objects)
        {
            if (obj == null)
                continue;

            Collider col = obj.GetComponentInChildren<Collider>();

            float objRadius = 0f;

            if (col != null)
                objRadius = col.bounds.extents.magnitude;

            float dist = Vector3.Distance(position, obj.transform.position);

            if (dist < radius + objRadius)
                return false;

            if (dist < minimumObjectDistance)
                return false;
        }

        return true;
    }
}