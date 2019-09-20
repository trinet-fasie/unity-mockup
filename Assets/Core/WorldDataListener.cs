using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TM.Errors;
using NLog;
using SmartLocalization;
using UnityEngine;
using UnityEngine.SceneManagement;
using TM.Data;
using TM.Data.ServerData;
using TM.ECS.Systems.Loader;
using TM.Public;
using TM.PUN;
using TM.Types;
using TM.UI;
using TM.UI.VRErrorManager;
using TM.WWW;
using VRTK;
using Logger = NLog.Logger;

namespace TM
{
    public class WorldDataListener : MonoBehaviour
    {
        #region PUBLIC VARS
        public static WorldDataListener Instance;
        public WorldLocationArguments WorldLocationArguments { get; set; }
        public WorldConfigurationArguments WorldConfigurationArguments { get; set; }
        public static Action OnUpdate { get; set; }
        #endregion

        #region PRIVATE VARS
        private int _lastLocationId;
        private bool _launch;
        private Logger _logger;
        private string _currentLoadedScene;
        #endregion

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

        private void Start()
        {
            Application.logMessageReceived += ErrorHelper.ErrorHandler;
            _logger = LogManager.GetCurrentClassLogger();
        }

        private void Update()
        {
            DoOnUpdate();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                new RequestTar("/logic.cs").OnFinish += codeResponse =>
                {
                    WorldData.LogicCode = ((ResponseTar) codeResponse).TextData;
                    Helper.ReloadLocationObjects();
                };
            }

        }

       
        private void DoOnUpdate()
        {
            try
            {
                OnUpdate?.Invoke();
            }
            catch (Exception e)
            {
                LogManager.GetCurrentClassLogger().Error("Error Invoke OnUpdate! " + e.Message + " " + e.StackTrace);
            }
        }

        public void ReadyToGetNewMessages()
        {
            _logger.Info($"Listener ready to listen!");
            _launch = false;
            
        }

        public void LoadConfigurationLocation(int locationId)
        {
            _lastLocationId = locationId;

            SceneLoaderSystem sceneLoaderSystem =
                new SceneLoaderSystem(Contexts.sharedInstance, null, locationId);
            sceneLoaderSystem.Initialize();
        }

        public int GetStartLocation(int worldConfigurationId)
        {
            int worldLocationId =  0;
            
            foreach (WorldConfiguration worldConfiguration in WorldData.WorldStructure.WorldConfigurations)
            {
                if (worldConfiguration.Id != worldConfigurationId)
                {
                    continue;
                }

                worldLocationId = worldConfiguration.StartWorldLocationId != 0 ? 
                    worldConfiguration.StartWorldLocationId : 
                    WorldData.WorldStructure.WorldLocations[0].Id;
            }

            return worldLocationId;
        }

        public void LoadWorldConfiguration(int worldConfigurationId, out int worldLocationId)
        {
            _logger.Info($"Loading world configuration {worldConfigurationId}...");
            GameStateData.ClearObjects();
            worldLocationId = 0;

            foreach (WorldConfiguration worldConfiguration in WorldData.WorldStructure.WorldConfigurations)
            {
                if (worldConfiguration.Id != worldConfigurationId)
                {
                    continue;
                }

                worldLocationId = GetStartLocation(worldConfigurationId);
            }

            if (worldLocationId == 0)
            {
                _logger.Error($"World configuration id = {worldConfigurationId} not found!");
            }

            int newWorldLocationId = worldLocationId;
            int oldWorldLocationId = WorldData.WorldLocationId;

            WorldData.Update(WorldData.WorldId, newWorldLocationId, worldConfigurationId);

            WorldData.OnLoadObjects = null;
            WorldData.ObjectsAreLoaded = false;

            WorldData.OnLoadObjects = delegate
            {
                WorldData.OnLoadLocation = delegate
                {
                    Helper.LoadLocationObjects(newWorldLocationId, oldWorldLocationId);
                    WorldData.OnLoadObjects = null;
                };
            };

        }

        private void LoadInNullConfigurationLocation(int worldLocationId)
        {
            _logger.Info("Loading location without world configuration...");
            var location = WorldData.WorldStructure.WorldLocations.GetWorldLocation(worldLocationId);

            if (location == null)
            {
                ErrorHelper.DisplayErrorByCode(TM.Errors.ErrorCode.LoadSceneError);
                _logger.Error($"WorldLocationId = {worldLocationId} not exists in  worldId = {WorldData.WorldId}!");
                return;
            }

            if (_lastLocationId != location.LocationId)
            {
                _logger.Info("Other worldLocationId. Need to load scene");
                _lastLocationId = location.LocationId;

                int newWorldLocationId = worldLocationId;
                int oldWorldLocationId = WorldData.WorldLocationId;

                WorldData.OnLoadLocation = delegate
                {
                    Helper.LoadLocationObjects(newWorldLocationId, oldWorldLocationId);
                    WorldData.OnLoadObjects = null;
                };

                SceneLoaderSystem sceneLoaderSystem =
                    new SceneLoaderSystem(Contexts.sharedInstance, null, location.LocationId);
                sceneLoaderSystem.Initialize();
            }
            else
            {
                _logger.Info("Same worldLocationId. Scene alredy loaded");
                 LoadObjectsInLoadedLocation(worldLocationId);
            }
        }

        private void LoadObjectsInLoadedLocation(int locationId)
        {
            _logger.Info("Loading objects in loaded location");
            int newWorldLocationId = locationId;
            int oldWorldLocationId = WorldData.WorldLocationId;
            Helper.LoadLocationObjects(newWorldLocationId, oldWorldLocationId);
        }
       

        public void LoadScene(string scenePath)
        {
            StartCoroutine(LoadSceneIfAllReady(scenePath));
        }

        private void LoadWorldDescriptor()
        {
            _logger.Info("Waiting to load WorldDescriptor...");
            WorldDescriptor worldDescriptor = FindObjectOfType<WorldDescriptor>();

            if (worldDescriptor == null)
            {
                ReturnLoadError("WorldDescriptor not found!");
                return;
            }

            if (worldDescriptor.PlayerSpawnPoint != null)
            {
                if (GameObjects.Instance != null)
                {
                    PlayerAnchorManager playerAnchorManager = FindObjectOfType<PlayerAnchorManager>();

                    if (playerAnchorManager == null)
                    {
                        playerAnchorManager = worldDescriptor.PlayerSpawnPoint.gameObject.AddComponent<PlayerAnchorManager>();
                    }
                    playerAnchorManager.ReloadPlayer(GameObjects.Instance.PlayerRig.GetComponentInChildren<VRTK_SDKSetup>().actualBoundaries);
                    return;
                }
                
                _logger.Info("Player Spawn Point was found! Adding PlayerAnchorManager");
                worldDescriptor.PlayerSpawnPoint.gameObject.AddComponent<PlayerAnchorManager>();
            }
            else
            {
                _logger.Error("Player Spawn Point not found in Location Config");
                ReturnLoadError("Player Spawn Point not found in Location Config");
            }

        }

        private void ReturnLoadError(string message)
        {
            _logger.Error("Error to load scene. " + message);
            GameObject go = new GameObject("Default player rig");
            go.transform.position = new Vector3(0, 1.8f, 0);
            go.transform.rotation = Quaternion.identity;
            go.AddComponent<PlayerAnchorManager>();
            StartCoroutine(ShowSpawnError());
        }

        private IEnumerator ShowSpawnError()
        {
            while (VRErrorManager.Instance == null)
            {
                yield return null;
            }

            VRErrorManager.Instance.Show(ErrorHelper.GetErrorDescByCode(TM.Errors.ErrorCode.SpawnPointNotFoundError));
            yield return true;
        }


        private IEnumerator LoadSceneIfAllReady(string scenePath)
        {
            _currentLoadedScene = Path.GetFileNameWithoutExtension(scenePath);

            while (!WorldData.ObjectsAreLoaded)
            {
                yield return null;
            }
            

            Debug.Log("Loading new Scene: " + Time.time);

            AsyncOperation loadSceneOperation = SceneManager.LoadSceneAsync(_currentLoadedScene, LoadSceneMode.Additive);

            while (!loadSceneOperation.isDone)
            {
                yield return null;
            }
            
            Debug.Log("Unloading old Scene: " + Time.time);
            
            AsyncOperation unloadSceneOperation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));
            
            while (!unloadSceneOperation.isDone)
            {
                yield return null;
            }
            
            LoadWorldDescriptor();
            _logger.Info("Location is loaded: " + Time.time);
            
            ReadyToGetNewMessages();

            yield return true;
        }

       // private void SceneManagerOnSceneLoaded(Scene arg0, LoadSceneMode loadSceneMode)
       // {
           // LoadWorldDescriptor();
        //    SceneManager.sceneLoaded -= SceneManagerOnSceneLoaded;
       // }

        public void StopLogicAndLoadScene(int newWorldLocationId, int oldWorldLocationId)
        {
            StartCoroutine(StopLogicAndLoadSceneCoroutine(newWorldLocationId, oldWorldLocationId));
        }

        private IEnumerator StopLogicAndLoadSceneCoroutine(int newWorldLocationId, int oldWorldLocationId)
        {
            GameStateData.ClearLogic();
            
            yield return new WaitForEndOfFrame();
            
            GameStateData.ClearObjects();
            
            if (newWorldLocationId == oldWorldLocationId)
            {

                //Hide everything
                
                Helper.LoadLocationObjects(newWorldLocationId, oldWorldLocationId);
               
                LoadWorldDescriptor();
                
                yield return new WaitForSeconds(1.0f);

                yield break;
            }

            Debug.Log("Loading loading Scene");

            AsyncOperation loadLoading = SceneManager.LoadSceneAsync("Loading", LoadSceneMode.Additive);

            while (!loadLoading.isDone)
            {
                yield return null;
            }

            Debug.Log("Unloading old Scene");
            //Hide everything
           // GameStateData.ClearObjects();

            AsyncOperation unloadSceneOperation = SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(0));

            while (!unloadSceneOperation.isDone)
            {
                yield return null;
            }

            Resources.UnloadUnusedAssets();

            Debug.Log("Preparing everything");

            GameObjects.Instance.PlayerRig.GetComponentInChildren<VRTK_SDKSetup>().actualBoundaries.transform.position = Vector3.zero;

            WorldData.Update(WorldData.WorldId, newWorldLocationId, WorldData.WorldConfigurationId);
            WorldData.OnLoadObjects = null;
            WorldData.ObjectsAreLoaded = false;
            Debug.Log("WorldDataListener.StopLogicAndLoadSceneCoroutine()");

            WorldData.OnLoadObjects = delegate
            {
                WorldData.OnLoadLocation = delegate
                {
                    Helper.LoadLocationObjects(newWorldLocationId, oldWorldLocationId);
                };
            };

            int locationId = WorldData.WorldStructure.WorldLocations.GetWorldLocation(newWorldLocationId).LocationId;

            SceneLoaderSystem
                sceneLoader =
                    new SceneLoaderSystem(Contexts.sharedInstance,
                        null,
                        locationId);
            sceneLoader.Initialize();



            yield return true;
        }

        public delegate void ObjectUpdate(GameEntity entity, ObjectDto dto);
        public event ObjectUpdate OnUpdateObject;
        
        public void UpdateObject(ObjectDto objectDto)
        {
            GameEntity entityObject = GameStateData.GetEntity(objectDto.InstanceId);
            entityObject.name.Value = objectDto.Name;
            OnUpdateObject?.Invoke(entityObject, objectDto);
        }

        public void LocationAdded(WorldLocationArguments newLocation)
        {
            WorldData.WorldStructure.WorldLocations.Add(newLocation.WorldLocation);
            WorldData.WorldStructure.UpdateOrAddLocationPrefab(newLocation.LocationPrefub);
        }
        
        public void LocationChanged(WorldLocationArguments changedLocation)
        {
            WorldData.WorldStructure.UpdateWorldLocation(changedLocation.WorldLocation);
            WorldData.WorldStructure.UpdateOrAddLocationPrefab(changedLocation.LocationPrefub);

            if (WorldData.WorldLocationId == changedLocation.WorldLocation.Id && WorldData.LocationId != changedLocation.WorldLocation.LocationId)
            {
                StopLogicAndLoadScene(changedLocation.WorldLocation.Id, WorldData.WorldLocationId);
            }
        }

        public void LocationDeleted(WorldLocationArguments deletedLocation)
        {
            if (WorldData.WorldLocationId == deletedLocation.WorldLocation.Id)
            {
                LogManager.GetCurrentClassLogger().Error("Current location was deleted");
                string message = LanguageManager.Instance.GetTextValue("CURRENT_LOCATION_DELETED");
                VRErrorManager.Instance.ShowFatal(message);
            }
            else
            {
                WorldData.WorldStructure.RemoveLocation(deletedLocation.WorldLocation);
            }
        }

        public void ConfigurationAdded(WorldConfiguration worldConfiguration)
        {
            WorldData.WorldStructure.WorldConfigurations.Add(worldConfiguration);
        }
        
        public void ConfigurationDeleted(WorldConfiguration worldConfiguration)
        {
            WorldData.WorldStructure.RemoveConfiguration(worldConfiguration);
        }
        
        public void ConfigurationChanged(WorldConfiguration worldConfiguration)
        {
            WorldData.WorldStructure.UpdateConfiguration(worldConfiguration);
        }

       
    }

    
}
