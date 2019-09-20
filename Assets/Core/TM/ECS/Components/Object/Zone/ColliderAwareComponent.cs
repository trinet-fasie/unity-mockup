using Entitas;
using TM.Public;

namespace TM.ECS.Components
{
    public class ColliderAwareComponent : IComponent
    {
        public IColliderAware Value;
    }
}