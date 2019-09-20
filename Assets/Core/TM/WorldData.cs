using System;
using System.Collections.Generic;
using NLog;
using TM.Data.ServerData;

namespace TM
{
    public static class WorldData
    {
        /// <summary>
        /// Current world id
        /// </summary>
        public static int WorldId { get; private set; }

        /// <summary>
        /// Current world location id
        /// </summary>
        public static int WorldLocationId { get; private set; }
        
        /// <summary>
        /// Current location id
        /// </summary>
        public static int LocationId => WorldStructure.WorldLocations.GetWorldLocation(WorldLocationId).LocationId;

        /// <summary>
        /// Current world configuration id
        /// </summary>
        public static int WorldConfigurationId { get; private set; }

        /// <summary>
        /// Object Id in hand
        /// </summary>
        public static int SelectedObjectIdToSpawn { get; set; } = 0;

        /// <summary>
        /// All objects are loaded
        /// </summary>
        public static bool ObjectsAreLoaded { get;  set; } = false;

        /// <summary>
        /// User modify something?
        /// </summary>
        public static bool ObjectsAreChanged { get; set; } = false;

        /// <summary>
        /// Current world structure data
        /// </summary>
        public static WorldStructure WorldStructure { get; set; }

        /// <summary>
        /// Action when objects are loaded
        /// </summary>
        public static Action OnLoadObjects { get; set; }

        /// <summary>
        /// Action when location is loaded
        /// </summary>
        public static Action OnLoadLocation { get; set; }

        /// <summary>
        /// Action when user save data
        /// </summary>
        public static Action OnSave { get; set; }

        public delegate void GameModeChangeHandler(GameMode newGameMode);
        public delegate void PlatformChangeHandler(PlatformMode newPlatformMode);

        public static event GameModeChangeHandler OnGameModeChange;
        public static event PlatformChangeHandler OnPlatformModeChange;

        private static GameMode _gm = GameMode.View;
        private static PlatformMode _platform = PlatformMode.Vr;

        public static string LogicCode;


        public static GameMode GameMode
        {
            get => _gm;
            set => GameModeChange(value);
        }
        
        public static PlatformMode PlatformMode
        {
            get => _platform;
            set => PlatformModeChange(value);
        }

        public static void Update(int worldId, int worldLocationId, int worldConfigurationId)
        {
            WorldId = worldId;
            WorldLocationId = worldLocationId;
            WorldConfigurationId = worldConfigurationId;
        }

        private static void PlatformModeChange(PlatformMode newValue)
        {
            if (newValue == _platform)
            {
                return;
            }

            _platform = newValue;
            OnPlatformModeChange?.Invoke(_platform);
            
        }

        private static void GameModeChange(GameMode newValue)
        {
            if (newValue == _gm)
            {
                return;
            }

            GameMode oldGm = _gm;
            _gm = newValue;
            OnGameModeChange?.Invoke(_gm);
            LogManager.GetCurrentClassLogger().Info($"<Color=Yellow>Game mode changed to {_gm.ToString()}</Color>");
            GameStateData.GameModeChanged(_gm, oldGm);

        }
    }

    public enum GameMode
    {
        View = 0,
    }

    public enum PlatformMode
    {
        Vr = 1
    }
}
