using Entitas;

namespace TM.ECS.Systems.Loader
{
    /// <summary>
    /// Calculate when objects loaded
    /// </summary>
    public sealed class LoadCounterSystem : IExecuteSystem 
    {
        private readonly IGroup<GameEntity> _loadCounter;

        public LoadCounterSystem(Contexts contexts)
        {
            _loadCounter = contexts.game.GetGroup(GameMatcher.LoadObjectsCounter);
        }

        public void Execute()
        {
            foreach (GameEntity entity in _loadCounter)
            {
                if (entity.loadObjectsCounter.PrefabsCount == entity.loadObjectsCounter.PrefabsLoaded && !WorldData.ObjectsAreLoaded)
                {
                    WorldData.ObjectsAreLoaded = true;
                    entity.loadObjectsCounter.LoadComplete = true;
                    WorldData.OnLoadObjects?.Invoke();
                }
            }
        }

       
    }
}
