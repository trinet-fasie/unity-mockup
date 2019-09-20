using System.Collections.Generic;
using Newtonsoft.Json;

// ReSharper disable once CheckNamespace
namespace TM.Data.ServerData
{
    public class LocationObjectsDto : IJsonSerializable
    {
        public int WorldLocationId { get; set; }
        public List<ObjectDto> WorldLocationObjects { get; set; }
    }

    public class ObjectDto : IJsonSerializable
    {
        public int Id { get; set; }
        public int InstanceId { get; set; }
        public string Name { get; set; }
        [JsonIgnore]
        public int? ParentId { get; set; }
        public int ObjectId { get; set; }
        public ObjectData Data { get; set; }
        public List<ObjectDto> WorldLocationObjects { get; set; }
    }



}