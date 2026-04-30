using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public int value = 20;

    public float lifetime = 60;
    private float timeAlive = 0;

    public ParticleSystem ammoEffect;

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
            Weapon gun = playerRoot.GetComponentInChildren<Weapon>();

            if (gun.ammo < gun.MaxAmmo)
            {
                gun.ammo = gun.ammo < gun.MaxAmmo - value ? gun.ammo + value : gun.MaxAmmo;

                Instantiate(ammoEffect, transform.position, Quaternion.Euler(0f, 0f, 0f));
                
                Destroy(gameObject);
            }
        } 
    }
}
