using Entities;
using Game.GameEngine.Ecs;
using System.Linq;
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

    private void Start()
    {
        character = GetComponent<CharacterEntity>();
        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (isInCombat)
        {
            // Если враг в бою, он стоит на месте и не двигается
            return;
        }

        // Проверяем наличие юнитов поблизости
        Collider[] units = Physics.OverlapSphere(transform.position, detectionRadius, unitLayer);
        if (units.Length > 0)
        {
            // Если юниты обнаружены, переходим в боевое состояние
            StartCombat(units);
            return;
        }
    }

    private void StartCombat(Collider[] units)
    {
        // Находим ближайшего юнита
        Transform nearestUnit = units
            .OrderBy(u => Vector3.Distance(transform.position, u.transform.position))
            .First()
            .transform;

        // Отправляем команду атаки
        character.SetData(new CommandRequest
        {
            type = CommandType.ATTACK_TARGET,
            args = nearestUnit.GetComponent<Entity>(),
            status = CommandStatus.IDLE
        });

        // Активируем боевое состояние
        isInCombat = true;

        // Анимация атаки или боевого состояния
        animator.SetInteger("State", 3); // Боевая анимация (например, 2 - атака)
    }

    public void EndCombat()
    {
        // Завершаем боевое состояние и остаемся на месте
        isInCombat = false;
        animator.SetInteger("State", 0); // Idle анимация
    }
}
