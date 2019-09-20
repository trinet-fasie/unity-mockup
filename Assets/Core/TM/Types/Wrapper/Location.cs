using System;
using System.Collections.Generic;
using NLog;
using TM.Data.ServerData;

// ReSharper disable once CheckNamespace
namespace TM 
{
    public static class Location
    {
        private static string _lastSid = String.Empty;

        public static void Load(string sid)
        {
            if (_lastSid == sid)
            {
                return;
            }
             
            var location = WorldData.WorldStructure.GetLocation(sid);
            
            if (location == null)
            {
                _lastSid = sid;
                LogManager.GetCurrentClassLogger().Error("Location not found!");
                return;
            }

            WorldDataListener.Instance.StopLogicAndLoadScene(location.Id, WorldData.WorldLocationId);
            _lastSid = string.Empty;
        }

        
    }
    
    public static class Configuration
    {
        private static string _lastSid = String.Empty;

        public static void Load(string sid)
        {
            if (_lastSid == sid)
            {
                return;
            }
             
            var worldConfiguration = WorldData.WorldStructure.GetConfiguration(sid);
            
            if (worldConfiguration == null)
            {
                _lastSid = sid;
                LogManager.GetCurrentClassLogger().Error("Location not found!");
                return;
            }

            int startLocation = WorldDataListener.Instance.GetStartLocation(worldConfiguration.Id); 

            WorldDataListener.Instance.StopLogicAndLoadScene(startLocation, WorldData.WorldLocationId);
            _lastSid = string.Empty;
        }  

        
    }
}
