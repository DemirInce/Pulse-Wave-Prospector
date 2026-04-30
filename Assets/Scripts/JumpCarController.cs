using UnityEngine;

public class JumpCarController : MonoBehaviour
{
    public enum FollowAxis
    {
        X,
        Z
    }
    public float particleTriggerDistance = 25f;
    public FollowAxis followAxis;

    private Transform player;
    private ParticleSystem ps;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        ps = GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (player == null) return;

        Vector3 pos = transform.position;

        if (followAxis == FollowAxis.X)
            pos.x = player.position.x;
        else if (followAxis == FollowAxis.Z)
            pos.z = player.position.z;

        transform.position = pos;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= particleTriggerDistance)
        {
            if (!ps.isPlaying) ps.Play();
        }
        else
        {
            if (ps.isPlaying) ps.Stop();
        }
    }
}