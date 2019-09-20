

namespace TM.ECS.Systems.Loader
{
    public sealed class LoaderSystems : Feature
    {
        public LoaderSystems(Contexts contexts)
        {
            if (!WorldData.ObjectsAreLoaded)
            {
                Add(new LoadCounterSystem(contexts));
            }          
        }
    }
}
