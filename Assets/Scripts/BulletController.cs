using Unity.Mathematics;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float lifetime = 5;
    public float damage = 5;
    public GameObject explosion;

    private float timeAlive = 0;

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;
        if(timeAlive >= lifetime)
        {
            Explode();
        }
    }

    private void Explode()
    {
        Instantiate(explosion, transform.position, Quaternion.identity);
        Destroy(gameObject);  
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Border"))
            return;
        if (timeAlive < 0.03f)
            return;
        if (other.CompareTag("Enemy") || other.CompareTag("Player"))
        {
            ShipController ship = other.GetComponentInParent<ShipController>();
            ship.TakeDamage(damage);
        }
        Explode(); 
    }
}
