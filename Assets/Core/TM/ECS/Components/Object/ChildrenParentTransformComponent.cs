using Entitas;
using UnityEngine;

namespace TM.ECS.Components
{
    public sealed class ChildrenParentTransformComponent : IComponent
    {
        public Transform Children;
        public Transform Parent;
    }
}
