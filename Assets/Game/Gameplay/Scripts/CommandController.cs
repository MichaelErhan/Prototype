using System.Linq;
using Game.GameEngine.Ecs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SampleProject
{
    public sealed class CommandController : MonoBehaviour
    {
        [SerializeField]
        private GameObject unitsParent; // Контейнер, где находятся все юниты

        private Entity[] unitPool;

        private void Start()
        {
            // Проверяем, что unitsParent не пустой
            if (unitsParent != null)
            {
                // Получаем все компоненты Entity, прикрепленные к дочерним объектам unitsParent
                unitPool = unitsParent.GetComponentsInChildren<Entity>(true); // true - включает неактивные объекты
                Debug.Log($"Found {unitPool.Length} units in the pool.");
            }
            else
            {
                Debug.LogWarning("Units parent is not assigned.");
            }
        }

        [Button]
        public void MoveToPosition(Transform point)
        {
            if (unitPool == null || unitPool.Length == 0)
            {
                Debug.LogWarning("No units found in the pool.");
                return;
            }

            Debug.Log($"Sending MoveToPosition command to {unitPool.Length} units.");

            foreach (var unit in unitPool)
            {
                if (unit != null)
                {
                    Debug.Log($"Unit {unit.name} receiving command.");
                    unit.SetData(new CommandRequest
                    {
                        type = CommandType.MOVE_TO_POSITION,
                        args = point.position,
                        status = CommandStatus.IDLE
                    });
                }
                else
                {
                    Debug.LogWarning("Found null unit in the pool.");
                }
            }
        }

        [Button]
        public void GatherResource(Entity resource)
        {
            if (unitPool == null || unitPool.Length == 0)
            {
                Debug.LogWarning("No units found in the pool.");
                return;
            }

            Debug.Log($"Sending GatherResource command to {unitPool.Length} units.");

            foreach (var unit in unitPool)
            {
                if (unit != null)
                {
                    Debug.Log($"Unit {unit.name} receiving command.");
                    unit.SetData(new CommandRequest
                    {
                        type = CommandType.GATHER_RESOURCE,
                        args = resource,
                        status = CommandStatus.IDLE
                    });
                }
                else
                {
                    Debug.LogWarning("Found null unit in the pool.");
                }
            }
        }

        [Button]
        public void Patrol()
        {
            if (unitPool == null || unitPool.Length == 0)
            {
                Debug.LogWarning("No units found in the pool.");
                return;
            }

            Debug.Log($"Sending Patrol command to {unitPool.Length} units.");

            foreach (var unit in unitPool)
            {
                if (unit != null)
                {
                    Debug.Log($"Unit {unit.name} receiving patrol command.");

                    // Находим компонент PatrolBehavior у юнита
                    PatrolBehavior patrolBehavior = unit.GetComponent<PatrolBehavior>();

                    if (patrolBehavior != null)
                    {
                        // Включаем компонент PatrolBehavior, если он выключен
                        patrolBehavior.enabled = true;

                        // Запускаем патрулирование (точки уже настроены в PatrolBehavior)
                        patrolBehavior.StartPatrolling();

                        Debug.Log($"Unit {unit.name} has started patrolling.");
                    }
                    else
                    {
                        Debug.LogWarning($"Unit {unit.name} does not have PatrolBehavior component.");
                    }
                }
                else
                {
                    Debug.LogWarning("Found null unit in the pool.");
                }
            }
        }

        [Button]
        public void Stop()
        {
            if (unitPool == null || unitPool.Length == 0)
            {
                Debug.LogWarning("No units found in the pool.");
                return;
            }

            Debug.Log("Sending Stop command to units.");

            foreach (var unit in unitPool)
            {
                if (unit != null)
                {
                    Debug.Log($"Unit {unit.name} receiving Stop command.");
                    unit.RemoveData<CommandRequest>();

                    PatrolBehavior patrolBehavior = unit.GetComponent<PatrolBehavior>();
                    if (patrolBehavior != null)
                    {
                        patrolBehavior.StopPatrol();

                        Debug.Log($"Unit {unit.name} has started patrolling.");
                    }
                    else
                    {
                        Debug.LogWarning($"Unit {unit.name} does not have PatrolBehavior component.");
                    }
                }
                else
                {
                    Debug.LogWarning("Found null unit in the pool.");
                }
            }
        }
    }
}
