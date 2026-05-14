using UnityEngine;
using System.Collections;
using UnityEngine.Assertions.Must;

public class TargetController : MonoBehaviour
{
    public Color targetColor = Color.gray;

    private Outline outline;
    private bool interacted = false;

    [Header("Rocket")]
    [SerializeField] private GameObject rocketPrefab;
    [SerializeField] private float launchSpeed = 10f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private float surfaceOffset = 0.2f;
    [SerializeField] private float attachDelay = 1f;

    [Header("Shake")]
    [SerializeField] private float shakeAmount = 0.08f;
    [SerializeField] private float shakeSpeed = 25f;

    private void Awake()
    {
        outline = TryGetComponent(out Outline ol) ? ol : gameObject.AddComponent<Outline>();

        outline.OutlineMode = Outline.Mode.OutlineVisible;
        outline.OutlineColor = Color.white;
        outline.OutlineWidth = 4;
        outline.enabled = false;
    }

    public void ShowOutline(bool show)
    {
        if (!interacted) outline.enabled = show;
    }

    public void LiftAway()
    {
        if (interacted) return;

        interacted = true;

        outline.enabled = false;

        RockController rock = GetComponent<RockController>();
        if (rock) rock.spinRate = 0;

        StartCoroutine(LaunchRocket());
    }

    private IEnumerator LaunchRocket()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (!player || !rocketPrefab) yield break;

        Vector3 asteroidCenter = transform.position;

        Vector3 toPlayer = (player.transform.position - asteroidCenter).normalized;

        toPlayer.y = 0f;
        toPlayer.Normalize();

        float radius = GetApproxRadius();

        Vector3 localOffset = toPlayer * (radius + surfaceOffset);
        Quaternion rotation = Quaternion.LookRotation(Vector3.up) * Quaternion.Euler(90f, 0f, 0f);
        GameObject rocket = Instantiate(rocketPrefab, asteroidCenter + localOffset, rotation);

        rocket.transform.SetParent(transform);

        yield return new WaitForSeconds(attachDelay);

        rocket.transform.SetParent(null);

        float elapsed = 0f;
        float currentSpeed = 0f;

        Vector3 asteroidBasePos = transform.position;
        Vector3 rocketBasePos = rocket.transform.position;

        while (elapsed < lifetime)
        {
            currentSpeed = Mathf.MoveTowards(
                currentSpeed,
                launchSpeed,
                acceleration * Time.deltaTime
            );

            Vector3 upwardMove = Vector3.up * currentSpeed * Time.deltaTime;

            asteroidBasePos += upwardMove;
            rocketBasePos += upwardMove;

            Vector3 shakeOffset =
                transform.right * Mathf.Sin(elapsed * shakeSpeed) * shakeAmount +
                transform.forward * Mathf.Cos(elapsed * shakeSpeed * 0.8f) * shakeAmount;

            transform.position = asteroidBasePos + shakeOffset;
            rocket.transform.position = rocketBasePos + shakeOffset;

            elapsed += Time.deltaTime;

            yield return null;
        }

        Destroy(rocket);
        enabled = false;
        gameObject.layer = 0;
        GameManager.Instance.UpdateTargets();
    }

    private float GetApproxRadius()
    {
        Renderer r = GetComponentInChildren<Renderer>();

        if (r) return r.bounds.extents.magnitude * 0.5f;

        return 1f;
    }
}