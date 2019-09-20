using Entitas;
using TM.Data;

namespace TM.ECS.Components.Events
{
    public sealed class SpawnAssetComponent : IComponent
    {
        /// <summary>
        /// Params for spawn
        /// </summary>
        public SpawnInitParams SpawnInitParams;

        /// <summary>
        /// Photon Id
        /// </summary>
        public int IdPhoton;
    }
}
