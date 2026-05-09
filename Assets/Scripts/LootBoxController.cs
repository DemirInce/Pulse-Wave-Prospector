using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class LootBoxController : MonoBehaviour
{
    public float triggerDistance = 25f;

    private Transform player;
    public ParticleSystem particles;
    public Vector2 particleSizeRange = new Vector2(0.5f, 2f);

    public ParticleSystem explosionPrefab;

    public List<GameObject> Pickups;

    public float countdownTimer = 5f;
    public float timer;
    private bool blown = false;

    private DropSpawner spawner;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        spawner = GetComponent<DropSpawner>();
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= triggerDistance)
        {
            timer -= Time.deltaTime;

            particles.Play();
            float ratio = (countdownTimer - timer) / countdownTimer;
            float scale = Mathf.Lerp(particleSizeRange.x, particleSizeRange.y, ratio);
            particles.transform.localScale = new Vector3(scale, scale, scale);

            if (timer <= 0 && !blown)
            {
                blown = true;
                particles.Stop();
                Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                spawner.SpawnDrops(400);
                gameObject.SetActive(false);
            }
        }
        else
        {
            timer = countdownTimer;
            particles.Stop();
            float scale = particleSizeRange.x; 
            particles.transform.localScale = new Vector3(scale, scale, scale);
        }
    }
}