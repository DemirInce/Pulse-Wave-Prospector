using System;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class EnemyController : MonoBehaviour
{
    private NavMeshAgent agent;
    private ShipController ship;

    public enum AIState
    {
        Patrol,
        Chase,
        Attack,
        Flee
    }

    public AIState currentState;

    [Header("References")]
    public Transform player;
    public Transform plane;
    public Weapon gun;

    [Header("Path Following")]
    public float cornerSkipDistance = 1.5f;
    public float arrivalRadius = 2f;

    [Header("Vision")]
    public float visionRange = 30f;
    public LayerMask obstacleMask;

    [Header("Memory")]
    public float memoryDuration = 5f;

    [Header("Patrol")]
    public float minPatrolTime = 2f;
    public float maxPatrolTime = 5f;

    [Header("Chase")]
    public float orbitRadius = 15f;

    [Header("Attack")]
    public float attackDistance = 5f;

    [Header("Flee")]
    public float fleeDistance = 40f;

    private float currentThrust;
    private float patrolTimer;
    private Vector3 patrolTarget;

    private float lastSeenTime;
    private Vector3 lastPlayerPos;
    private bool hasDestination = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ship = GetComponent<ShipController>();

        agent.updatePosition = false;
        agent.updateRotation = false;
    }

    void Start()
    {
        currentState = AIState.Patrol;
        PickNewPatrolPoint();
    }

    void Update()
    {
        agent.nextPosition = transform.position;

        // Flee chance under 50% HP
        if (currentState != AIState.Patrol && currentState != AIState.Flee && ship.health < ship.maxHealth * 0.5f)
        {
            float healthPercent = ship.health / ship.maxHealth;
            float fleeChance = 1f - 2f * healthPercent;

            if (UnityEngine.Random.value < fleeChance * Time.deltaTime)
            {
                currentState = AIState.Flee;
                hasDestination = false;
            }
        }

        switch (currentState)
        {
            case AIState.Patrol:
                HandlePatrol();
                break;

            case AIState.Chase:
                HandleChase();
                break;

            case AIState.Attack:
                HandleAttack();
                break;

            case AIState.Flee:
                HandleFlee();
                break;
        }

        FollowPath();
        if(currentState != AIState.Attack) gun.SetFiring(false);
        ship.Tick();
    }

    // ---------------- STATE LOGIC ----------------

    void HandlePatrol()
    {
        patrolTimer -= Time.deltaTime;

        if (!hasDestination || patrolTimer <= 0f)
        {
            PickNewPatrolPoint();
            hasDestination = true;
        }

        if (CanSeePlayer())
        {
            currentState = AIState.Chase;
            lastSeenTime = Time.time;
            lastPlayerPos = player.position;
        }
    }

    void HandleChase()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            lastPlayerPos = player.position;
        }

        // Lost player → go back to patrol after memory expires
        if (Time.time - lastSeenTime > memoryDuration)
        {
            currentState = AIState.Patrol;
            return;
        }

        // Too close → back off
        if (dist < attackDistance * 0.8f)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 target = transform.position + dir * orbitRadius;
            agent.SetDestination(target);
            return;
        }

        // Close enough → attack
        if (dist <= orbitRadius)
        {
            currentState = AIState.Attack;
            return;
        }

        // Otherwise chase player
        agent.SetDestination(lastPlayerPos);
    }

    void HandleAttack()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (CanSeePlayer())
        {
            lastSeenTime = Time.time;
            lastPlayerPos = player.position;
        }

        // Lost player
        if (Time.time - lastSeenTime > memoryDuration)
        {
            gun.SetFiring(false);
            currentState = AIState.Patrol;
            return;
        }

        // TOO CLOSE → disengage
        if (dist < attackDistance)
        {
            gun.SetFiring(false);

            Vector3 away = (transform.position - player.position).normalized;
            Vector3 disengagePoint = transform.position + away * orbitRadius;

            agent.SetDestination(disengagePoint);
            currentState = AIState.Chase;
            return;
        }

        // Attack run: fly directly at player
        agent.SetDestination(player.position);

        // Fire while approaching
        if (dist < orbitRadius)
            gun.SetFiring(true);
        else
            gun.SetFiring(false);
    }

    void HandleFlee()
    {
        if (!hasDestination)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            Vector3 fleeTarget = transform.position + dir * fleeDistance;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(fleeTarget, out hit, 20f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }

            hasDestination = true;
        }

        float dist = Vector3.Distance(transform.position, agent.destination);

        if (dist < arrivalRadius)
        {
            hasDestination = false;
            currentState = AIState.Patrol;
        }
    }

    void PickNewPatrolPoint()
    {
        patrolTarget = GetRandomNavMeshPoint();
        agent.SetDestination(patrolTarget);
        patrolTimer = UnityEngine.Random.Range(minPatrolTime, maxPatrolTime);
    }

    Vector3 GetRandomNavMeshPoint()
    {
        float width = plane.localScale.x * 10f;
        float length = plane.localScale.z * 10f;

        for (int i = 0; i < 30; i++)
        {
            float x = UnityEngine.Random.Range(-width / 2f, width / 2f);
            float z = UnityEngine.Random.Range(-length / 2f, length / 2f);

            Vector3 randomPoint = new Vector3(
                plane.position.x + x,
                plane.position.y,
                plane.position.z + z
            );

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 10f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return transform.position;
    }

    bool CanSeePlayer()
    {
        Vector3 origin = transform.position + Vector3.up * 1.5f;
        Vector3 target = player.position + Vector3.up * 1.5f;

        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (dist > visionRange)
            return false;

        dir.Normalize();

        if (Physics.Raycast(origin, dir, dist, obstacleMask))
            return false;

        return true;
    }

    // ---------------- PATH FOLLOWING ----------------

    void FollowPath()
    {
        if (agent.pathPending || agent.path.corners.Length == 0)
            return;

        Vector3 finalTarget = agent.path.corners[^1];

        if (Vector3.Distance(transform.position, finalTarget) < arrivalRadius)
        {
            currentThrust = 0f;
            ship.velocity = Vector3.zero;
            agent.ResetPath();
            ship.ApplyThrust(currentThrust);
            return;
        }

        Vector3 target = finalTarget;

        foreach (var corner in agent.path.corners)
        {
            if (Vector3.Distance(transform.position, corner) > cornerSkipDistance)
            {
                target = corner;
                break;
            }
        }

        Vector3 direction = target - transform.position;
        direction.y = 0f;

        if (direction.magnitude < 0.1f)
            return;

        direction.Normalize();

        ship.RotateTowards(direction);

        float alignment = Vector3.Dot(transform.forward, direction);
        float thrust = Mathf.Clamp01(alignment);

        currentThrust = Mathf.Lerp(currentThrust, thrust, 5f * Time.deltaTime);

        ship.ApplyThrust(currentThrust);
    }
}
