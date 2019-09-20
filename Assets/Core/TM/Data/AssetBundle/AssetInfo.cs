
using System.Collections.Generic;
using TM.Types;

// ReSharper disable once CheckNamespace
namespace TM.Data
{
    public class AssetInfo : IJsonSerializable
    {
        public string AssetName { get; set; }
        public List<string> Assembly { get; set; }
        public List<string> Resources { get; set; }
    }
}
