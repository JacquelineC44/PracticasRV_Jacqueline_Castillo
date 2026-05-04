using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public Transform player;

    [Header("Velocidades")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float waitTime = 1.5f;
    private int currentPoint = 0;
    private float waitCounter;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 70f;
    public LayerMask obstacleMask;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPoint].position);
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            ChasePlayer();
        }
        else
        {
            Patrol();
        }
    }

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void Patrol()
    {
        agent.speed = patrolSpeed;
        agent.isStopped = false;

        if (patrolPoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            waitCounter += Time.deltaTime;

            if (waitCounter >= waitTime)
            {
                currentPoint = (currentPoint + 1) % patrolPoints.Length;
                agent.SetDestination(patrolPoints[currentPoint].position);
                waitCounter = 0;
            }
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 directionToPlayer = player.position - origin;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > viewAngle / 2)
            return false;

        if (Physics.Raycast(origin, directionToPlayer.normalized, distanceToPlayer, obstacleMask))
            return false;

        return true;
    }
}