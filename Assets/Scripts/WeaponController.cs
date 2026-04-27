using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour
{
    public Transform[] nozzles;
    public GameObject bulletPrefab;
    public float fireRate = 0.1f;
    public float bulletSpeed = 20f;

    private int currentNozzle = 0;
    private Coroutine firingCoroutine;

    public void SetFiring(bool firing)
    {
        if (firing)
        {
            if (firingCoroutine == null)
                firingCoroutine = StartCoroutine(FireLoop());
        }
        else
        {
            if (firingCoroutine != null)
            {
                StopCoroutine(firingCoroutine);
                firingCoroutine = null;
            }
        }
    }

    IEnumerator FireLoop()
    {
        while (true)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    void Shoot()
    {
        if (nozzles.Length == 0) return;

        Transform nozzle = nozzles[currentNozzle];

        GameObject bullet = Instantiate(
            bulletPrefab,
            nozzle.position,
            nozzle.rotation
        );

        Vector3 velocity = GetComponentInParent<ShipController>().velocity;

        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forward = nozzle.forward;
            forward.y = 0f;
            forward.Normalize();

            Vector3 horizontalVelocity = new Vector3(velocity.x, 0f, velocity.z);

            float forwardSpeed = Vector3.Dot(horizontalVelocity, forward);

            if (forwardSpeed > 0f)
            {
                rb.linearVelocity = forward * bulletSpeed + horizontalVelocity;
            }
            else
            {
                rb.linearVelocity = forward * bulletSpeed * 0.5f;
            }
        }

        ParticleSystem flash = nozzle.GetComponentInChildren<ParticleSystem>();
        if (flash != null)
        {
            flash.Play();
        }

        currentNozzle = (currentNozzle + 1) % nozzles.Length;
    }
}