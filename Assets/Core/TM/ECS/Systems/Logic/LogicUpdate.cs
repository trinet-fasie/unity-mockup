using Entitas;
using TM.Data.ServerData;

namespace TM.ECS.Systems.Group
{
    public sealed class LogicUpdate : IExecuteSystem
    {
        private readonly IGroup<GameEntity> _entities;
        private readonly string _cs;
        private readonly int _locationId;
        public LogicUpdate(Contexts contexts, string cs, int locationId)
        {
            _entities = contexts.game.GetGroup(GameMatcher.Logic);
            _cs = cs;
            _locationId = locationId;
        }

        public void Execute()
        {
            foreach (GameEntity entity in _entities)
            {
                entity.ReplaceCsCode(_cs);
            }
        }
    }
}
