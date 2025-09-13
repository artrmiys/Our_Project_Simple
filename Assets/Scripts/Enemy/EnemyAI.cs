using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // ��� ������ � NavMesh

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;   // ����� �������
    public float visionRange = 10f;    // ������ "������"
    public float visionAngle = 60f;    // ���� ������
    public float chaseSpeed = 5f;      // �������� ��� �������������
    public float patrolSpeed = 2f;     // �������� ��� �������
    public Transform player;           // ������ �� ������

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

        // ���� ����� ����� �� ���� ���������
        if (Vector3.Distance(transform.position, player.position) > visionRange * 1.5f)
        {
            currentState = State.Patrol;
            agent.speed = patrolSpeed;
            agent.SetDestination(patrolPoints[currentPoint].position);
        }
    }
}
