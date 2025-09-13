using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Для работы с NavMesh

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;   // точки патруля
    public float visionRange = 10f;    // радиус "зрения"
    public float visionAngle = 60f;    // угол зрения
    public float chaseSpeed = 5f;      // скорость при преследовании
    public float patrolSpeed = 2f;     // скорость при патруле
    public Transform player;           // ссылка на игрока

    private int currentPoint = 0;
    private NavMeshAgent agent;
    private enum State { Patrol, Chase }
    private State currentState = State.Patrol;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.speed = patrolSpeed;
        agent.SetDestination(patrolPoints[currentPoint].position);
    }

    void Update()
    {
        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                LookForPlayer();
                break;

            case State.Chase:
                Chase();
                break;
        }
    }

    void Patrol()
    {
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentPoint = (currentPoint + 1) % patrolPoints.Length;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }

    void LookForPlayer()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);

        if (Vector3.Distance(transform.position, player.position) < visionRange && angle < visionAngle / 2)
        {
            currentState = State.Chase;
            agent.speed = chaseSpeed;
        }
    }

    void Chase()
    {
        agent.SetDestination(player.position);

        // если игрок вышел из зоны видимости
        if (Vector3.Distance(transform.position, player.position) > visionRange * 1.5f)
        {
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
}
