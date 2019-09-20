using System;
using System.Collections.Generic;
using NLog;
using TM.Models.Data;

namespace TM.Data.ServerData
{
    public class WorldStructure : IJsonSerializable
    {
        public int WorldId { get; set; }
        public string WorldName { get; set; }
        public List<WorldLocation> WorldLocations { get; set; }
        public List<WorldConfiguration> WorldConfigurations { get; set; }
        public List<LocationPrefub> Locations { get; set; }
        public List<PrefabObject> Objects { get; set; }
    }

    public class WorldLocationWithLocationPrefab : IJsonSerializable
    {
        public WorldLocation WorldLocation;
        public LocationPrefub Location;
    }

    public class WorldLocation : IJsonSerializable
    {
        /// <summary>
        /// Instance id of location
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Guid of location in world structure
        /// </summary>
        public string Sid { get; set; }
        /// <summary>
        /// Location name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Location prefab id
        /// </summary>
        public int LocationId { get; set; }
        public string LocationGuid { get; set; }
        /// <summary>
        /// C# logic code
        /// </summary>
        public EditorData EditorData { get; set; }
        /// <summary>
        /// Location objects
        /// </summary>
        public List<ObjectDto> WorldLocationObjects { get; set; }
    }

    public class WorldLocationArguments
    {
        public WorldLocation WorldLocation = null;
        public LocationPrefub LocationPrefub = null;
        public StateLocation State;
         
        public enum StateLocation
        {
            Added, Deleted, Changed
        }
    }
    
    public class WorldConfigurationArguments
    {
        public WorldConfiguration WorldConfiguration = null;
        public StateConfiguration State;
         
        public enum StateConfiguration
        {
            Added, Deleted, Changed
        }
    }

    public class WorldConfiguration : IJsonSerializable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Sid { get; set; }
        public int WorldId { get; set; }
        public int StartWorldLocationId { get; set; }
        public string CreatedAt { get; set; }
        public string UpdatedAt { get; set; }
    }

    public class LocationPrefub
    {
        public int Id { get; set; }
        public string Guid { get; set; }
        public string Name { get; set; }
        public LocationResourses Resources { get; set; }
    }

    public class ObjectData
    {
        public Dictionary<int, TransformDT> Transform { get; set; }
    }

    public class JointData
    {
        public Dictionary<int, JointConnetionsData> JointConnetionsData { get; set; }
    }

    public class JointConnetionsData
    {
        /// <summary>
        /// Connected object instance id
        /// Какой объект подключен
        /// </summary>
        public int ConnectedObjectInstanceId { get; set; }

        /// <summary>
        /// Connected object point
        /// К какой точке
        /// </summary>
        public int ConnectedObjectJointPointId { get; set; }
        
        /// <summary>
        /// Joint point force lock
        /// </summary>
        public bool ForceLocked { get; set; }
    }

    public class LocationResourses
    {
        public string Bundle { get; set; }
        public string Manifest { get; set; }
        public string Config { get; set; }
        public string Icon { get; set; }
    }

    public class EditorData
    {
        public string Blockly { get; set; }
    }

    


    #region Ex
    public static class WorldStructureEx
    {
        public static WorldLocation GetWorldLocation(this List<WorldLocation> self, int id)
        {
            foreach (WorldLocation location in self)
            {
                if (location.Id == id)
                {
                    return location;
                }
            }

            return null;
        }

        public static WorldLocation GetWorldLocation(this List<WorldLocation> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (WorldLocation location in self)
            {
                Guid sidGuid = new Guid(location.Sid);

                if (sidGuid == guid) return location;
            }

            return null;
        }
        
        public static WorldConfiguration GetWorldConfiguration(this List<WorldConfiguration> self, string sid)
        {
            Guid guid = new Guid(sid);

            foreach (WorldConfiguration worldConfiguration in self)
            {
                Guid sidGuid = new Guid(worldConfiguration.Sid);

                if (sidGuid == guid) return worldConfiguration;
            }

            return null;
        }

        public static WorldConfiguration GetWorldConfigurationLocation(this List<WorldConfiguration> self, int worldLocationId)
        {
            foreach (WorldConfiguration worldConfiguration in self)
            {
                if (worldConfiguration.Id == worldLocationId) return worldConfiguration;
            }

            return null;
        }

        public static List<string> GetNames(this List<WorldConfiguration> self)
        {
            List<string> names = new List<string>();
            foreach (WorldConfiguration worldConfiguration in self)
            {
                names.Add(worldConfiguration.Name);
            }

            return names;
        }

        public static int GetId(this List<WorldConfiguration> self, string name)
        {
             
            foreach (WorldConfiguration worldConfiguration in self)
            {
                if (worldConfiguration.Name == name) return worldConfiguration.Id;
            }

            return 0;
        }

        public static WorldLocation GetLocation(this WorldStructure self, string locationSid)
        {
            var worldLocation = self.WorldLocations.GetWorldLocation(locationSid);
            return worldLocation;
        }
        
        public static WorldConfiguration GetConfiguration(this WorldStructure self, string configurationId)
        {
            var worldConfiguration = self.WorldConfigurations.GetWorldConfiguration(configurationId);
            return worldConfiguration;
        }

        public static void UpdateEntities(List<ObjectDto> objects)
        {
            foreach (ObjectDto dto in objects)
            {
                GameEntity entity = GameStateData.GetEntity(dto.InstanceId);
                entity.ReplaceIdServer(dto.Id);
                entity.ReplaceName(dto.Name);
                UpdateEntities(dto.WorldLocationObjects);
            }
        }

        public static void UpdateLocationObjects(this WorldLocation self, List<ObjectDto> objects)
        {
            UpdateEntities(objects);
            self.WorldLocationObjects = objects;
            WorldData.ObjectsAreChanged = false;
            LogManager.GetCurrentClassLogger().Info($"Location objects {WorldData.WorldLocationId} was updated in structure!");
        }

        public static LocationPrefub GetLocation(this List<LocationPrefub> self, int id)
        {
            foreach (LocationPrefub prefub in self)
            {
                if (prefub.Id == id) return prefub;
            }

            return null;
        }

        public static PrefabObject GetById(this List<PrefabObject> self, int id)
        {
            foreach (PrefabObject o in self)
            {
                if (o.Id == id)
                {
                    return o;
                }
            }

            return null;
        }

        public static void RemoveLocation(this WorldStructure self, WorldLocation deletedLocation)
        {
            WorldLocation result = null; 
            foreach (WorldLocation worldLocation in self.WorldLocations)
            {
                    
                if (worldLocation.Id == deletedLocation.Id)
                {
                    result = worldLocation;
                }
            }

            if (result != null)
            {
                self.WorldLocations.Remove(result);
            }
        }
        
        public static void UpdateWorldLocation(this WorldStructure self, WorldLocation changedLocation)
        {
            for (int i = 0; i < self.WorldLocations.Count; i++)
            {
                if (self.WorldLocations[i].Id == changedLocation.Id)
                {
                    self.WorldLocations[i] = changedLocation;
                }
            }
        }
        
        public static void UpdateOrAddLocationPrefab(this WorldStructure self, LocationPrefub changedLocationPrefub)
        {
            for (int i = 0; i < self.Locations.Count; i++)
            {
                if (self.Locations[i].Id == changedLocationPrefub.Id)
                {
                    self.Locations[i] = changedLocationPrefub;
                    return;
                }
            }
            
            self.Locations.Add(changedLocationPrefub);
        }
        
        public static void RemoveConfiguration(this WorldStructure self, WorldConfiguration deletedConfiguration)
        {
            WorldConfiguration result = null; 
            foreach (WorldConfiguration worldConfiguration in self.WorldConfigurations)
            {
                    
                if (worldConfiguration.Id == deletedConfiguration.Id)
                {
                    result = worldConfiguration;
                }
            }

            if (result != null)
            {
                self.WorldConfigurations.Remove(result);
            }
        }
        
        public static void UpdateConfiguration(this WorldStructure self, WorldConfiguration changedConfiguration)
        {
            for (int i = 0; i < self.WorldConfigurations.Count; i++)
            {
                if (self.WorldConfigurations[i].Id == changedConfiguration.Id)
                {
                    self.WorldConfigurations[i] = changedConfiguration;
                }
            }
        }
    }
    #endregion

}
