using Entitas;
using TM.Data;
using TM.Data.ServerData;

namespace TM.ECS.Components.Events
{
    public sealed class LoadGroupAssetComponent : IComponent
    {
        public ObjectDto Value;

        /// <summary>
        /// Photon Id
        /// </summary>
        public int IdPhoton;

    }
}
