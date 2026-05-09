using UnityEngine;
using UnityEngine.AI;

public class EnemyAi : MonoBehaviour
{
    public Transform player;
    [Header("Respawn del jugador")]
    public Transform playerStartPoint;

    [Header("Velocidades")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 5f;
    public float searchSpeed = 3f;

    [Header("Patrulla")]
    public Transform[] patrolPoints;
    public float waitTime = 1.5f;
    private int currentPoint = 0;
    private float waitCounter;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 70f;
    public LayerMask obstacleMask;

    [Header("Busqueda")]
    public float lostSightDelay = 3f;
    public float searchDistance = 8f;
    public float searchTime = 3f;

    //avisar que te esgtan buscando
    [Header("Luz ambiente")]
    public Color normalAmbientColor;
    public Color alertAmbientColor = new Color(1f, 0f, 0.03f);
    private static int enemiesInAlert = 0;
    private bool isAlerting = false;

    private float lostSightCounter;
    private float searchCounter;
    private Vector3 searchTarget;

    private NavMeshAgent agent;

    enum State
    {
        Patrol,
        Chase,
        LostSight,
        Search
    }

    private State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
            agent.SetDestination(patrolPoints[currentPoint].position);
        normalAmbientColor = RenderSettings.ambientLight;
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Debug.Log("Patrullaje");
                if (CanSeePlayer())
                {
                    currentState = State.Chase;
                }
                else
                {
                    Patrol();
                }
                break;

            case State.Chase:
                Debug.Log("esta siguiendo");
                if (CanSeePlayer())
                {
                    lostSightCounter = 0f;
                    ChasePlayer();
                }
                else
                {
                    currentState = State.LostSight;
                    lostSightCounter = 0f;
                }
                break;

            case State.LostSight:
                Debug.Log("Lo perdio");
                if (CanSeePlayer())
                {
                    currentState = State.Chase;
                }
                else
                {
                    lostSightCounter += Time.deltaTime;

                    if (lostSightCounter >= lostSightDelay)
                    {
                        StartSearch();
                    }
                }
                break;

            case State.Search:
                Debug.Log("Lo esta buscando");
                if (CanSeePlayer())
                {
                    currentState = State.Chase;
                }
                else
                {
                    SearchForward();
                }
                break;
            default:
                break;
        }
        UpdateAmbientLight();
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

    void ChasePlayer()
    {
        agent.speed = chaseSpeed;
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    void StartSearch()
    {
        currentState = State.Search;
        agent.speed = searchSpeed;
        searchCounter = 0f;

        Vector3 forwardTarget = transform.position + transform.forward * searchDistance;

        if (NavMesh.SamplePosition(forwardTarget, out NavMeshHit hit, searchDistance, NavMesh.AllAreas))
        {
            searchTarget = hit.position;
            agent.SetDestination(searchTarget);
        }
        else
        {
            currentState = State.Patrol;
        }
    }

    void SearchForward()
    {
        searchCounter += Time.deltaTime;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            currentState = State.Patrol;
            ReturnToPatrol();
        }

        if (searchCounter >= searchTime)
        {
            currentState = State.Patrol;
            ReturnToPatrol();
        }
    }

    void ReturnToPatrol()
    {
        agent.speed = patrolSpeed;

        if (patrolPoints.Length > 0)
        {
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector3 origin = transform.position + Vector3.up;
        Vector3 target = player.position + Vector3.up;
        Vector3 directionToPlayer = target - origin;

        float distanceToPlayer = directionToPlayer.magnitude;

        if (distanceToPlayer > viewDistance)
            return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);

        if (angle > viewAngle / 2f)
            return false;

        if (Physics.Raycast(origin, directionToPlayer.normalized, out RaycastHit hit, viewDistance))
        {
            if (hit.transform == player)
                return true;
        }

        return false;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform == player || other.transform.IsChildOf(player))
        {
            RespawnPlayer();
        }
    }

    void RespawnPlayer()
    {
        CharacterController cc = player.GetComponent<CharacterController>();

        if (cc != null)
            cc.enabled = false;

        player.position = playerStartPoint.position;
        player.rotation = playerStartPoint.rotation;

        if (cc != null)
            cc.enabled = true;

        currentState = State.Patrol;
        ReturnToPatrol();
    }
    void UpdateAmbientLight()
    {
        bool shouldAlert = currentState == State.Chase || currentState == State.Search;

        if (shouldAlert && !isAlerting)
        {
            enemiesInAlert++;
            isAlerting = true;
        }
        else if (!shouldAlert && isAlerting)
        {
            enemiesInAlert--;
            isAlerting = false;
        }

        if (enemiesInAlert > 0)
        {
            RenderSettings.ambientLight = alertAmbientColor;
        }
        else
        {
            RenderSettings.ambientLight = normalAmbientColor;
        }
    }
    private void OnDisable()
    {
        if (isAlerting)
        {
            enemiesInAlert--;
            isAlerting = false;
        }
    }
}