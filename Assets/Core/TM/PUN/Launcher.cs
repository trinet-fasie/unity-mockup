using System.Collections.Generic;
using NLog;
using NLogger;
using SmartLocalization;
using UnityEngine;
using TM.Data;
using TM.Data.ServerData;
using TM.ECS.Systems.Loader;
using TM.Experemental;
using TM.UI;
using TM.WWW;

namespace TM.PUN
{
    public class Launcher : MonoBehaviour
    {
        #region PUBLIC VARS

        public static Launcher Instance;

        [Tooltip("The UI Loader Anime")]
        public LoaderAnime LoaderAnime;


        public string Language;
        public bool LoadTarFile;
        public string TarFilePath;

        #endregion

        #region PRIVATE VARS

        private bool _isConnecting;
        string _gameVersion = "1";
        private NLog.Logger _logger;

        #endregion

        private void OnApplicationQuit()
        {
            Debug.Log("Application ending after " + Time.time + " seconds");
        }

        private void Awake()
        {
            _logger = LogManager.GetCurrentClassLogger();
            _logger.Info("Launcher started");

            #region Magic code

#pragma warning disable 219
            dynamic inside = "Create Microsoft.CSharp.dll in project folder";
            _logger.Info(inside);
#pragma warning restore 219

            #endregion

            Instance = this;
            RuntimeCompiler.Init();
            NLoggerSettings.Init();
        }

        private void Start()
        {
            LoaderAnime.gameObject.transform.position = new Vector3(0, (float) Screen.height / 1000 - 1, 0);
            LanguageManager.SetDontDestroyOnLoad();
            Contexts.sharedInstance.game.DestroyAllEntities();

            
            Settings.CreateTarFileSettings(TarFilePath);
            StartLoadFile();
        }

        private void StartLoadFile()
        {
            LogManager.GetCurrentClassLogger().Info("File name = " + Settings.Instance().TarFile);
            WorldData.GameMode = GameMode.View;

            if (LoaderAnime != null)
            {
                LoaderAnime.StartLoaderAnimation();
            }

            new RequestTar("/index.json").OnFinish += response =>
            {
                string jsonWorldStructure = ((ResponseTar) response).TextData;
                WorldData.WorldStructure = jsonWorldStructure.JsonDeserialize<WorldStructure>();

                new RequestTar("/logic.cs").OnFinish += codeResponse =>
                {
                    WorldData.LogicCode = ((ResponseTar) codeResponse).TextData;
                    
                    WorldData.WorldStructure.WorldConfigurations = new List<WorldConfiguration>() { new WorldConfiguration() };
                    
                    var worldConfigurationId = WorldData.WorldStructure.WorldConfigurations[0].Id;
                    LoadConfigurationFromTar(worldConfigurationId);
                    
                };
                
            };
        }

        private void LoadConfigurationFromTar(int worldConfigurationId)
        {
            LoaderAnime.StartLoaderAnimation();
            LoaderSystems loaderSystems = new LoaderSystems(Contexts.sharedInstance);
            loaderSystems.Initialize();
            WorldDataListener.OnUpdate = () => loaderSystems.Execute();
            int worldLocationId;
            DataAdapter.LoadObjects();
            WorldDataListener.Instance.LoadWorldConfiguration(worldConfigurationId, out worldLocationId);
            int locationId = WorldData.WorldStructure.WorldLocations.GetWorldLocation(worldLocationId).LocationId;

            SceneLoaderSystem sceneLoaderSystem =
                new SceneLoaderSystem(Contexts.sharedInstance, null, locationId);
            sceneLoaderSystem.Initialize();
        }

        private void LogFeedback(string message)
        {
           
        }


        public void LoadLocation(int locationId)
        {
            LauncherErrorManager.Instance.Hide();
            Contexts.sharedInstance.game.DestroyAllEntities();
            _isConnecting = true;

            if (LoaderAnime != null)
            {
                LoaderAnime.StartLoaderAnimation();
            }

            
                LogFeedback(LanguageManager.Instance.GetTextValue("SCENE_LOADING"));

                LoaderSystems loaderSystems = new LoaderSystems(Contexts.sharedInstance);
                loaderSystems.Initialize();
                WorldDataListener.OnUpdate = () => loaderSystems.Execute();
                DataAdapter.LoadObjects();

                SceneLoaderSystem
                    sceneLoader =
                        new SceneLoaderSystem(Contexts.sharedInstance,
                            null,
                            locationId);
                sceneLoader.Initialize();
            
        }
    }
}
