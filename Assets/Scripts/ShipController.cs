using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ShipController : MonoBehaviour
{
    [Header("Movement")]
    public float acceleration = 15f;
    public float drag = 5f;
    public float rotationSpeed = 8f;
    public float fixedY = 0f;

    [Header("Thruster")]
    public ParticleSystem[] thrusterParticles;

    [Header("Health")]
    public float maxHealth = 100f;
    public float health;
    public ParticleSystem explosionPrefab;

    private bool isDead = false;

    private int nextThresholdIndex = 0;
    private float[] thresholds;

    private DamageEffects dmgEffects;

    private CharacterController controller;

    public Vector3 velocity;
    private float currentThrust;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        dmgEffects = GetComponent<DamageEffects>();

        health = maxHealth;

        // Setup damage thresholds (same logic as enemy)
        thresholds = new float[5];
        for (int i = 0; i < 5; i++)
        {
            thresholds[i] = maxHealth * (5 - i) / 6f;
        }

        if (dmgEffects != null)
            dmgEffects.ResetPoints();
    }

    // ---------------- MOVEMENT ----------------

    public void ApplyThrust(float thrust)
    {
        currentThrust = thrust;

        Vector3 forward = transform.forward;
        forward.y = 0f;
        forward.Normalize();

        velocity += forward * thrust * acceleration * Time.deltaTime;
    }

    public void ApplyDrag()
    {
        Vector3 horizontal = new Vector3(velocity.x, 0f, velocity.z);
        horizontal = Vector3.Lerp(horizontal, Vector3.zero, drag * Time.deltaTime);

        velocity = new Vector3(horizontal.x, 0f, horizontal.z);
    }

    public void Move()
    {
        controller.Move(velocity * Time.deltaTime);

        Vector3 pos = transform.position;
        pos.y = fixedY;
        transform.position = pos;
    }

    public void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;
        if (direction.sqrMagnitude < 0.001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            rotationSpeed * 360f * Time.deltaTime
        );
    }

    public void UpdateThruster()
    {
        if (thrusterParticles == null || thrusterParticles.Length == 0) return;

        bool shouldPlay = currentThrust > 0.1f;

        foreach (var ps in thrusterParticles)
        {
            if (ps == null) continue;

            if (shouldPlay)
            {
                if (!ps.isPlaying)
                    ps.Play();
            }
            else
            {
                if (ps.isPlaying)
                    ps.Stop();
            }
        }
    }

    public void Tick()
    {
        ApplyDrag();
        Move();
        UpdateThruster();
    }

    // ---------------- DAMAGE ----------------

    public void TakeDamage(float damage)
    {
        if (isDead) return;

        health -= damage;

        // Trigger staged damage effects
        while (nextThresholdIndex < thresholds.Length &&
               health <= thresholds[nextThresholdIndex])
        {
            if (dmgEffects != null)
                dmgEffects.SpawnEffect();

            nextThresholdIndex++;
        }

        if (health <= 0f)
        {
            Die();
        }
    }

    private void Die()
    {
        isDead = true;

        if (explosionPrefab != null)
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}