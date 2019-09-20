namespace TM.ECS.Systems.Compiler
{
    public sealed class CompilerSystems : Feature
    {
        public CompilerSystems(Contexts contexts)
        {
            Add(new RuntimeCompilerSystem(contexts));
            Add(new TypeAnalizatorSystem(contexts));
            Add(new AddLogicSystem(contexts));
        }
    }
}
