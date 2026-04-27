using UnityEngine;
namespace EfeTuran
{

    public class AsteroidSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] asteroidPrefabs;
        [SerializeField] private int asteroidCount = 50;
        [SerializeField] private float spawnRadius = 100f;
        [SerializeField] private Vector3 spawnCenter = Vector3.zero;

        [Header("Physics Settings")]
        [SerializeField] private float minForce = 1f;
        [SerializeField] private float maxForce = 5f;
        [SerializeField] private float minTorque = 0.1f;
        [SerializeField] private float maxTorque = 0.5f;
        [SerializeField] private float asteroidMass = 1f;
        [SerializeField] private bool useGravity = false;

        private void Start()
        {
            SpawnAsteroids();
        }

        private void SpawnAsteroids()
        {
            for (int i = 0; i < asteroidCount; i++)
            {
                Vector3 randomPosition = spawnCenter + Random.insideUnitSphere * spawnRadius;
                Quaternion randomRotation = Random.rotation;

                GameObject selectedAsteroid = asteroidPrefabs[Random.Range(0, asteroidPrefabs.Length)];
                GameObject asteroid = Instantiate(selectedAsteroid, randomPosition, randomRotation);

                float randomScale = Random.Range(0.5f, 2f);
                asteroid.transform.localScale = Vector3.one * randomScale;

                Rigidbody rb = asteroid.GetComponent<Rigidbody>();
                if (rb == null)
                {
                    rb = asteroid.AddComponent<Rigidbody>();
                    asteroid.AddComponent<BoxCollider>();
                }

                rb.isKinematic = false;
                rb.constraints = RigidbodyConstraints.None;
                rb.mass = asteroidMass;
                rb.useGravity = useGravity;
                rb.linearDamping = 0f;
                rb.angularDamping = 0f;

                Vector3 randomDirection = Random.insideUnitSphere.normalized;
                float randomForceMagnitude = Random.Range(minForce, maxForce);
                rb.AddForce(randomDirection * randomForceMagnitude, ForceMode.Impulse);

                Vector3 randomTorqueAxis = Random.insideUnitSphere.normalized;
                float randomTorqueMagnitude = Random.Range(minTorque, maxTorque);
                rb.AddTorque(randomTorqueAxis * randomTorqueMagnitude, ForceMode.Impulse);
            }
        }
    }

}