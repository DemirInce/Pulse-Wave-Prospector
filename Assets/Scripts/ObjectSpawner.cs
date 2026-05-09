using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    [Header("Setup")]
    public Transform plane;              
    public GameObject[] objectPrefabs;     

    [Header("Spawn Settings")]
    public int Count = 50;

    [Header("Spacing")]
    public float extraSpacing = 1f;

    private RockSpawner rockSpawner;

    void Awake()
    {
        rockSpawner = GetComponent<RockSpawner>();
        SpawnObjects();
    }

    void SpawnObjects()
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
                radius = rockSpawner.GetPrefabRadius(prefab) + extraSpacing;
                attempts++;
            }
            while (!rockSpawner.IsValidPosition(spawnPos, radius) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
                continue;

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

            obj.SetActive(false);

            var gm = GameManager.Instance;
            gm.currArea.objects.Add(obj);

            rockSpawner.placedPositions.Add(spawnPos);
            rockSpawner.placedRadii.Add(radius);
        }
    }
}