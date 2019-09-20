namespace TM
{
    public interface ILogic
    {
        void SetCollection(WrappersCollection collection);
        void Initialize();
        void Update();
        void Events();
    }
}
