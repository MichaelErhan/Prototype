using Entities;
using Game.GameEngine.Ecs;
using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField]
    private float detectionRadius = 10f; // Радиус обнаружения юнитов
    [SerializeField]
    private LayerMask unitLayer; // Слой для юнитов

    private CharacterEntity character;
    private Animator animator;
    private bool isInCombat = false; // Флаг боевого состояния
    private Transform currentTarget = null; // Текущий враг, с которым ведется бой

    private bool isDead = false; // Флаг для проверки смерти

    private void Awake()
    {
        character = GetComponent<CharacterEntity>();
        animator = GetComponentInChildren<Animator>();
    }

    public void Disable()
    {
        enabled = false;
    }

    private void Update()
    {
        if (isDead) return; // Если юнит мёртв, ничего не делаем

        if (isInCombat)
        {
            if (currentTarget == null || !IsValidEnemy(currentTarget))
            {
                // Если враг в бою, проверяем, жив ли текущий враг
                ClearEnemyData(); // Очищаем данные о текущем враге

                // Оптимизация: используем цикл вместо LINQ
                Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRadius, unitLayer);
                Collider nearestEnemy = null;
                float nearestDistance = float.MaxValue;

                foreach (var enemy in enemies)
                {
                    if (IsValidEnemy(enemy.transform))
                    {
                        float distance = Vector3.Distance(transform.position, enemy.transform.position);
                        if (distance < nearestDistance)
                        {
                            nearestEnemy = enemy;
                            nearestDistance = distance;
                        }
                    }
                }

                if (nearestEnemy != null)
                {
                    StartCombat(nearestEnemy); // Передаем Collider
                }
                else
                {
                    EndCombat();
                }
                return;
            }
        }

        // Проверяем наличие юнитов поблизости
        Collider[] units = Physics.OverlapSphere(transform.position, detectionRadius, unitLayer);
        if (units.Length > 0)
        {
            // Находим ближайшего юнита
            Collider nearestUnit = null;
            float nearestDistance = float.MaxValue;

            foreach (var unit in units)
            {
                if (IsValidEnemy(unit.transform))
                {
                    float distance = Vector3.Distance(transform.position, unit.transform.position);
                    if (distance < nearestDistance)
                    {
                        nearestUnit = unit;
                        nearestDistance = distance;
                    }
                }
            }

            if (nearestUnit != null)
            {
                StartCombat(nearestUnit); // Передаем Collider
            }
            return;
        }
    }

    public bool IsValidEnemy(Transform enemy)
    {
        return enemy != null && enemy.gameObject.activeInHierarchy;
    }

    private void ClearEnemyData()
    {
        currentTarget = null;
        character.RemoveData<CommandRequest>(); // Удаляем текущую команду
    }

    private void StartCombat(Collider unit)
    {
        // Получаем Transform врага из Collider
        Transform nearestUnitTransform = unit.transform;

        // Проверяем, если ближайшая цель валидна
        if (!IsValidTarget(nearestUnitTransform))
        {
            return; // Если цель недоступна, выходим
        }

        // Запоминаем цель боя
        currentTarget = nearestUnitTransform;

        // Отправляем команду атаки
        character.SetData(new CommandRequest
        {
            type = CommandType.ATTACK_TARGET,
            args = nearestUnitTransform.GetComponent<Entity>(),
            status = CommandStatus.IDLE
        });

        // Активируем боевое состояние
        isInCombat = true;

        // Анимация атаки или боевого состояния
        animator.SetInteger("State", 3); // Боевая анимация
    }

    public void EndCombat()
    {
        // Завершаем боевое состояние и остаемся на месте
        isInCombat = false;
        currentTarget = null; // Очищаем цель
        animator.SetInteger("State", 0); // Idle анимация
    }

    // Проверка на валидность цели (например, жива ли она)
    private bool IsValidTarget(Transform target)
    {
        return target != null && target.gameObject.activeInHierarchy;
    }
}
