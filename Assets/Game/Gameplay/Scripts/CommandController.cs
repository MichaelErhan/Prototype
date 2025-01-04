using System.Linq;
using Game.GameEngine.Ecs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SampleProject
{
    public sealed class CommandController : MonoBehaviour
    {
        [SerializeField]
        private Entity entityPlayer;
        [SerializeField]
        private Entity entityEnemy;

        [Button]
        public void MoveToPosition(Transform point)
        {
            this.entityPlayer.SetData(new CommandRequest
            {
                type = CommandType.MOVE_TO_POSITION,
                args = point.position,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void AttackTargetEnemy(Entity target)
        {
            this.entityPlayer.SetData(new CommandRequest
            {
                type = CommandType.ATTACK_TARGET,
                args = target,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void AttackTargetPlayer(Entity target)
        {
            this.entityEnemy.SetData(new CommandRequest
            {
                type = CommandType.ATTACK_TARGET,
                args = target,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void GatherResource(Entity resource)
        {
            this.entityPlayer.SetData(new CommandRequest
            {
                type = CommandType.GATHER_RESOURCE,
                args = resource,
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void Patrol(Transform[] points)
        {
            this.entityPlayer.SetData(new CommandRequest
            {
                type = CommandType.PATROL_BY_POINTS,
                args = points.Select(it => it.position).ToList(),
                status = CommandStatus.IDLE
            });
        }

        [Button]
        public void Stop()
        {
            this.entityPlayer.RemoveData<CommandRequest>();
            this.entityEnemy.RemoveData<CommandRequest>();
        }
    }
}


