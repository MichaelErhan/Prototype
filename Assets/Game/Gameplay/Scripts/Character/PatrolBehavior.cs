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

        // Проверка, есть ли точки патрулирования
        if (patrolPoints.Length == 0)
        {
            Debug.LogWarning("Не заданы точки патрулирования!");
            return;
        }

        StartPatrolling();
    }

    private void Update()
    {
        // Если враги есть и мы в патрулировании, останавливаем патрулирование и начинаем бой
        if (isPatrolling)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
            if (enemies.Length > 0)
            {
                // Обнаружены враги, прекращаем патрулирование и начинаем бой
                StopPatrol();
                AttackNearestEnemy(enemies);
                return;
            }
        }

        // Если в бою, но врагов больше нет или они мертвы, возвращаемся к патрулированию
        if (isInCombat)
        {
            Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, enemyLayer);
            if (enemies.Length == 0 || currentEnemy == null || !currentEnemy.gameObject.activeInHierarchy)
            {
                EndCombat();
                StartPatrolling();
                return;
            }
        }

        // Если мы в патрулировании и врагов нет
        if (isPatrolling)
        {
            PatrolMovement();
        }
    }

    private void PatrolMovement()
    {
        if (patrolPoints.Length == 0) return;

        // Если мы не ждем, начинаем движение
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= patrolWaitTime)
            {
                isWaiting = false; // Заканчиваем ожидание
                waitTimer = 0f; // Сбрасываем таймер ожидания
                MoveToNextPoint(); // Переход к следующей точке
            }
            animator.SetInteger("State", 0); // Idle анимация
            return;
        }

        Vector3 targetPosition = patrolPoints[currentPatrolIndex].position;
        Vector3 directionToTarget = (targetPosition - transform.position).normalized;

        // Поворачиваем персонажа в направлении движения
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }

        // Двигаемся к текущей точке
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        animator.SetInteger("State", 1); // Анимация ходьбы

        // Проверяем, достигли ли текущей точки
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            isWaiting = true; // Начинаем ожидание по завершении движения
        }
    }

    private void MoveToNextPoint()
    {
        // Переход к следующей точке патрулирования
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public void StartPatrolling()
    {
        animator.SetInteger("State", 0); // Idle анимация
        isPatrolling = true;
        currentPatrolIndex = 0; // Начинаем с первой точки
        isWaiting = false; // Не ждем
        waitTimer = 0f; // Сбрасываем таймер ожидания

        // Обновление данных патрулирования персонажа
        character.RemoveData<PatrolData>();
    }

    public void StopPatrol()
    {
        animator.SetInteger("State", 0); // Idle анимация
        isPatrolling = false; // Останавливаем патрулирование
        isWaiting = false; // Устанавливаем флаг ожидания
        waitTimer = 0f; // Сбрасываем таймер ожидания
    }

    private void AttackNearestEnemy(Collider[] enemies)
    {
        if (isInCombat)
            return;  // Если уже в бою, не начинаем новую атаку

        isInCombat = true; // Устанавливаем флаг боевого состояния

        // Находим ближайшего врага
        Transform nearestEnemy = enemies
            .OrderBy(e => Vector3.Distance(transform.position, e.transform.position))
            .First()
            .transform;

        currentEnemy = nearestEnemy; // Запоминаем нового врага

        // Отправляем команду атаки
        character.SetData(new CommandRequest
        {
            type = CommandType.ATTACK_TARGET,
            args = currentEnemy.GetComponent<Entity>(),
            status = CommandStatus.IDLE
        });

        // Анимация атаки
        animator.SetInteger("State", 3); // Боевая анимация
    }

    private void EndCombat()
    {
        isInCombat = false; // Снимаем флаг боевого состояния
        currentEnemy = null; // Убираем врага
        animator.SetInteger("State", 0); // Возвращаем в Idle анимацию
    }

    private void OnDrawGizmosSelected()
    {
        // Визуализация радиуса обнаружения
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        // Визуализация маршрута патрулирования
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
            // Замыкаем маршрут
            if (patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
            {
                Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
            }
        }
    }
}
