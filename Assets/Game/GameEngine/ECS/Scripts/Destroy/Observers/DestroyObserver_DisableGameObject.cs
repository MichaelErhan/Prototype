using GameECS;
using UnityEngine;

namespace Game.GameEngine.Ecs
{
    public sealed class DestroyObserver_DisableGameObject : IEcsObserver<DestroyEvent>
    {
        private readonly EcsEmitter<DestroyEvent> destroyEmitter;
        private readonly EcsPool<GameObjectComponent> gameObjectPool;

        void IEcsObserver<DestroyEvent>.Handle(int entity, DestroyEvent destroyEvent)
        {
            ref var enemyComponent = ref this.gameObjectPool.GetComponent(entity);
            var enemyObject = enemyComponent.value;

            var enemyBehavior = enemyObject.GetComponent<EnemyBehavior>();
            if (enemyBehavior != null)
            {
                enemyBehavior.Disable(); // Отключаем EnemyBehavior
            }

            // Проверяем и отключаем компонент PatrolBehavior, если он есть
            var patrolBehavior = enemyObject.GetComponent<PatrolBehavior>();
            if (patrolBehavior != null)
            {
                patrolBehavior.Disable(); // Отключаем PatrolBehavior
            }

            ref var goComponent = ref this.gameObjectPool.GetComponent(entity);
            var gameObject = goComponent.value;

            var animator = gameObject.GetComponentInChildren<Animator>();
            if (animator != null)
            {
                if (animator.GetInteger("State") != 5)
                {
                    animator.SetInteger("State", 5);
                }
            }

            var deactivationHelper = gameObject.GetComponent<DeactivationHelper>();
            if (deactivationHelper == null)
            {
                deactivationHelper = gameObject.AddComponent<DeactivationHelper>();
            }

            deactivationHelper.DeactivateAfterDelay(gameObject, 0.7f);
        }
    }
}
