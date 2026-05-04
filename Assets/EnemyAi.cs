//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.AI;

//public class EnemyAi : MonoBehaviour
//{
//    public Transform Player;
//    private NavMeshAgent agent;
//    Start is called before the first frame update
//    void Start()
//    {
//        agent = GetComponent<NavMeshAgent>();
//    }

//    Update is called once per frame
//    void Update()
//    {
//        if (Player != null)
//        {
//            agent.SetDestination(Player.position);
//        }

//    }
//}
using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public Transform player;

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

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void Update()
    {
        if (CanSeePlayer())
        {
            agent.SetDestination(player.position);
        }
        else
        {
            Patrol();
        }
    }

    void Patrol()
    {
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

        Vector3 directionToPlayer = player.position - transform.position;
        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > viewAngle / 2)
            return false;

        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }

        return false;
    }
}

