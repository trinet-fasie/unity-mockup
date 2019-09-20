using System.Collections.Generic;
using Entitas;

namespace TM.ECS.Components
{
    public sealed class InputControlsComponent : IComponent
    {
        public Dictionary<int, InputController> Values;
    }
}
