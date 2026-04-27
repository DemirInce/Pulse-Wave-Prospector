using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;

    public float height = 12f;
    public float distance = -10f;

    public float smoothTime = 0.15f;

    private Vector3 velocity;

    void Start()
    {
        Transform child = transform.Find("Darkness Quad");
        child.gameObject.SetActive(true);
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = new Vector3(
            target.position.x,
            height,
            target.position.z + distance
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            desiredPosition,
            ref velocity,
            smoothTime
        );

        // FIXED rotation — no jitter
        transform.rotation = Quaternion.Euler(60f, 0f, 0f);
    }
}