namespace TM.ECS.Systems
{
    public sealed class LogicSystems : Feature
    {
        public LogicSystems(Contexts contexts)
        {
            Add(new LogicExecuteSystem(contexts));
        }
    }
}
