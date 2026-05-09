using UnityEngine;

public class FuelPickup : MonoBehaviour
{
    public int value = 4;

    public float lifetime = 60;
    private float timeAlive = 0;

    public ParticleSystem fuelEffect;

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
            Transform playerRoot = other.transform.root;
            ShipController ship = playerRoot.GetComponentInChildren<ShipController>();

            if (ship.fuel < ship.maxFuel)
            {
                ship.fuel = ship.fuel < ship.maxFuel - value ? ship.fuel + value : ship.maxFuel;

                Instantiate(fuelEffect, transform.position, Quaternion.Euler(0f, 0f, 0f));
                
                Destroy(gameObject);
            }
        } 
    }
}
