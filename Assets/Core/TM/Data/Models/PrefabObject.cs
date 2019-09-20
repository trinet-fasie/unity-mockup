

// ReSharper disable once CheckNamespace

using System;

namespace TM.Data
{
    public class PrefabObjectResources
    {
        public string Bundle { get; set; }
        public string Manifest { get; set; }
        public string Config { get; set; }
        public string Icon { get; set; }
        public string DllPath { get; set; }
    }

    public class PrefabObject : IJsonSerializable
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public bool Embedded { get; set; }
        public Config Config { get; set; }
        public PrefabObjectResources Resources { get; set; }
    }

  

}