using TM.ECS.Systems.Loader;
using TM.ECS.Systems.Compiler;

namespace TM.ECS.Systems
{
    public sealed class GameSystems : Feature
    {
        public GameSystems(Contexts contexts)
        {
            Add(new SpawnAssetSystem(contexts));
            Add(new ZoneControlSystem(contexts));
            Add(new CompilerSystems(contexts));
            Add(new LogicSystems(contexts));

        }
    }
}
