using System;
using System.IO;
using TM.Errors;
using Newtonsoft.Json;
using NLog;
using SmartLocalization;
using UnityEngine;
using TM.Data.ServerData;
using TM.UI;

namespace TM
{
    public class Settings
    {
        public string ApiHost { get; set; }
        public string Language { get; set; }
        public string TarFile { get; set; }
        public string TempFolder {get; set; }
        public string DebugFolder { get; set; }
        public bool HighlightEnabled;
        
        //haptics settings
        public bool TouchHapticsEnabled;
        public bool GrabHapticsEnabled;
        public bool UseHapticsEnabled;

        private static Settings _instance;

        public static void ReadTestSettings()
        {
            string json = File.ReadAllText(Application.dataPath + "/StreamingAssets/settings.txt");
            _instance = JsonConvert.DeserializeObject<Settings>(json, new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.None });
        }

        public static Settings Instance()
        {
            return _instance;
        }

        public static void CreateTarFileSettings(string file)
        { 
            _instance = new Settings
            {
                TarFile = file,
                TempFolder = Application.dataPath + "\\..\\" + file,
                HighlightEnabled = true
            };
      
        }

        public static void GreateDebugSettings(string path)
        {
            _instance = new Settings
            {
                DebugFolder = path,
                HighlightEnabled = true,
                TouchHapticsEnabled = true,
                GrabHapticsEnabled = true,
                UseHapticsEnabled = true
            };

        }
    }
}
