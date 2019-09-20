using Entitas;

namespace TM.ECS.Systems
{
    public sealed class LogicExecuteSystem : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        public LogicExecuteSystem(Contexts contexts)
        {
            _entities = contexts.game.GetGroup(GameMatcher.Logic);
        }

        public void Execute()
        {
            if (!WorldData.ObjectsAreLoaded)
            {
                return;
            }

            foreach (GameEntity entity in _entities.GetEntities())
            {
                entity.logic.Value.ExecuteLogic();
            }
        }
    }
}
