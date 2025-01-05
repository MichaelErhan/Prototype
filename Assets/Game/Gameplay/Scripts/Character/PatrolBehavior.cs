using Entities;
using Game.GameEngine.Ecs;
using System.Linq;
using UnityEngine;

public class PatrolBehavior : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 10f; // Радиус обнаружения врагов
    [SerializeField]
    private LayerMask enemyLayer; // Слой для врагов
    [SerializeField]
    private Transform[] patrolPoints; // Массив точек патрулирования
    [SerializeField]
    private float patrolWaitTime = 2f; // Время ожидания в каждой точке
    [SerializeField]
    private float moveSpeed = 5f; // Скорость перемещения

    private CharacterEntity character;
    private Animator animator;
    private bool isPatrolling = true; // Патрулирование активно
    private int currentPatrolIndex = 0; // Индекс текущей точки
    private float waitTimer = 0f; // Таймер ожидания на точках
    private bool isWaiting = false; // Флаг ожидания на точках
    private Transform currentEnemy = null; // Текущий враг, с которым ведется бой

    private bool isInCombat = false; // Флаг для контроля боевого состояния

    private void Start()
    {
        character = GetComponent<CharacterEntity>();
        animator = GetComponentInChildren<Animator>();

        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("Не заданы точки патрулирования!");
            return;
        }

        StartPatrolling();
    }

    private void Update()
    {
        if (isPatrolling)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
            if (enemies.Any(e => IsValidEnemy(e.transform)))
            {
                StopPatrol();
                AttackNearestEnemy(enemies);
                return;
            }
        }

        if (isInCombat)
        {
            if (currentEnemy == null || !IsValidEnemy(currentEnemy))
            {
                ClearEnemyData(); // Очищаем данные о текущем враге

                Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer)
                                           .Where(e => IsValidEnemy(e.transform))
                                           .ToArray();
                if (enemies.Length > 0)
                {
                    AttackNearestEnemy(enemies);
                }
                else
                {
                    EndCombat();
                    StartPatrolling();
                }
                return;
            }
        }

        if (isPatrolling)
        {
            PatrolMovement();
        }
    }

    private void PatrolMovement()
    {
        if (patrolPoints.Length == 0) return;

        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                MoveToNextPoint();
            }
            animator.SetInteger("State", 0);
            return;
        }

        Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        animator.SetInteger("State", 1);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isWaiting = true;
        }
    }
    private void ClearEnemyData()
    {
        currentEnemy = null;
        character.RemoveData<CommandRequest>(); // Удаляем текущую команду
    }

    private void MoveToNextPoint()
    {
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void StartPatrolling()
    {
        animator.SetInteger("State", 0);
        isPatrolling = true;
        currentPatrolIndex = 0;
        isWaiting = false;
        waitTimer = 0f;
        character.RemoveData<PatrolData>();
    }

    public void StopPatrol()
    {
        animator.SetInteger("State", 0);
        isPatrolling = false;
        isWaiting = false;
        waitTimer = 0f;
    }

    private void AttackNearestEnemy(Collider[] enemies)
    {
        if (enemies == null || enemies.Length == 0)
        {
            EndCombat();
            return;
        }

        Transform nearestEnemy = enemies
            .Where(e => IsValidEnemy(e.transform))
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .FirstOrDefault()?.transform;

        if (nearestEnemy == null)
        {
            EndCombat();
            return;
        }

        currentEnemy = nearestEnemy;
        isInCombat = true;

        character.SetData(new CommandRequest
        {
            type = CommandType.ATTACK_TARGET,
            args = currentEnemy.GetComponent<Entity>(),
            status = CommandStatus.IDLE
        });

        animator.SetInteger("State", 3); // Анимация атаки
    }

    private void EndCombat()
    {
        isInCombat = false;
        currentEnemy = null;
        animator.SetInteger("State", 0);
    }

    private bool IsValidEnemy(Transform enemy)
    {
        return enemy != null && enemy.gameObject.activeInHierarchy;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                }
            }

            if (patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }
    }
}