using Entitas;

namespace TM.ECS.Systems.Compiler
{
    /// <summary>
    /// Load group? Add Group logic
    /// </summary>
    public sealed class AddLogicSystem : IExecuteSystem //, ICleanupSystem
    {
        private readonly IGroup<GameEntity> _logics;
        public AddLogicSystem(Contexts contexts)
        {
            _logics = contexts.game.GetGroup(GameMatcher.AllOf( GameMatcher.Type, GameMatcher.ChangeGroupLogic, GameMatcher.Logic));
        }

        public void Execute()
        {
            if (!WorldData.ObjectsAreLoaded)
            {
                return;
            }

            foreach (GameEntity objectEntity in _logics.GetEntities())
            {
                if (objectEntity.type.Value != null)
                {
                    objectEntity.logic.Value.UpdateGroupLogic(objectEntity.type.Value);
                    objectEntity.RemoveChangeGroupLogic();
                }
                
            }
        }

    }
}
