using System.Collections.Generic;

namespace TM.Data.AssetBundle
{
    public class SceneData : IJsonSerializable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public string AssetBundleLabel { get; set; }
        public List<object> DllNames { get; set; }
    }
}
