using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    public int value = 20;

    public float lifetime = 60;
    private float timeAlive = 0;

    public ParticleSystem healthEffect;

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if(timeAlive >= lifetime)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && timeAlive > 1f)
        {
            ShipController ship = other.GetComponentInParent<ShipController>();

            if (ship.health < ship.maxHealth)
            {
                ship.health = ship.health < ship.maxHealth - value ? ship.health + value : ship.maxHealth;

                Instantiate(healthEffect, transform.position, Quaternion.Euler(0f, 0f, 0f));

                Destroy(gameObject);
            }
        } 
    }
}
