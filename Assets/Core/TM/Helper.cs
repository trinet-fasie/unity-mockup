using System;
using System.Collections.Generic;
using TM.Errors;
using TM.Data;
using DesperateDevs.Utils;
using NLog;
using UnityEngine;
using TM.Data.ServerData;
using TM.New.PUN;
using TM.Public;
using TM.Types;
using TM.UI;
using TM.UI.VRErrorManager;
using TM.UI.VRMessageManager;
using TM.WWW;
using VRTK;
using Object = UnityEngine.Object;

namespace TM
{
    /// <summary>
    ///     Класс помошник для разных методов. По мере наполнения, надо будет группировать методы по логиге и разбивать на
    ///     классы
    /// </summary>
    public static class Helper
    {
        private static GameObject _spawnView;
        private static NLog.Logger _logger = LogManager.GetCurrentClassLogger();
        
        public static event Action<ObjectController> ObjectControllerCreated;


        public static void LoadLocationObjects(int newWorldLocationId, int oldWorldLocationId)
        {
            _logger.Info($"Loading location objects for world location {newWorldLocationId}");
            ParentManager.Instance.ParentCommand = ParentCommand.None;

            GameStateData.ClearObjects();
            WorldData.ObjectsAreChanged = false;
            WorldLocation location = WorldData.WorldStructure.WorldLocations.GetWorldLocation(newWorldLocationId);
            LogicInstance logicInstance = new LogicInstance(newWorldLocationId);
            GameStateData.RefreshLogic(logicInstance, WorldData.LogicCode);
            CreateSpawnEntities(location.WorldLocationObjects, newWorldLocationId);

        }

        public static void ReloadLocationObjects()
        {
            _logger.Info($"Reloading location objects for world location {WorldData.WorldLocationId}");
            ParentManager.Instance.ParentCommand = ParentCommand.None;
            GameStateData.ClearObjects();
            WorldData.ObjectsAreChanged = false;
            WorldLocation location = WorldData.WorldStructure.WorldLocations.GetWorldLocation(WorldData.WorldLocationId);
            LogicInstance logicInstance = new LogicInstance(WorldData.WorldLocationId);
            GameStateData.RefreshLogic(logicInstance, WorldData.LogicCode);
            CreateSpawnEntities(location.WorldLocationObjects, WorldData.WorldLocationId);
            WorldDataListener.Instance.ReadyToGetNewMessages();
        }

        private static void CreateSpawnEntities(List<ObjectDto> locationObjects, int groupId)
        {
            foreach (ObjectDto o in locationObjects)
            {
                CreateSpawnEntity(o, groupId);
            }
        }

        private static void CreateSpawnEntity(ObjectDto o, int locationId, int? parentId = null)
        {
            bool embedded = false;
            PrefabObject prefabObject = WorldData.WorldStructure.Objects.GetById(o.ObjectId);

            if (prefabObject != null)
            {
                embedded = prefabObject.Embedded;
            }

            SpawnInitParams param = new SpawnInitParams
            {
                IdLocation = locationId,
                IdInstance = o.InstanceId,
                IdObject = o.ObjectId,
                IdServer = 0,
                Name = o.InstanceId.ToString(),
                ParentId = parentId
            };

            if (o.Data != null)
            {
                param.Transforms = o.Data.Transform;
            }

            Spawner.Instance.SpawnAsset(param);

        }

        public static LineRenderer CreateLineRenderer(GameObject go)
        {
            if (WorldData.GameMode == GameMode.View)
            {
                return null;
            }

            LineRenderer lineRenderer = go.AddComponent<LineRenderer>();
            Color c1 = new Color(0, 112, 255, 109);
            Color c2 = new Color(26, 255, 4, 82);

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.widthMultiplier = 0.005f;

            // A simple 2 color gradient with a fixed alpha of 1.0f.
            float alpha = 1.0f;
            Gradient gradient = new Gradient();

            gradient.SetKeys(
                new[] {new GradientColorKey(c1, 0.0f), new GradientColorKey(c2, 1.0f)},
                new[] {new GradientAlphaKey(alpha, 0.0f), new GradientAlphaKey(alpha, 1.0f)}
            );
            lineRenderer.colorGradient = gradient;

            return lineRenderer;
        }

        private static void AddCastComponent(Object unityObject, GameEntity entity)
        {
            GameObject gameObject = unityObject as GameObject;

            if (gameObject != null)
            {
                entity.AddGameObject(gameObject);
            }
        }

        public static bool LoadAssetInfo(string json, ref AssetInfo assetInfo, ref GameEntity loadCounter, PrefabObject o)
        {
            LauncherErrorManager launcherErrorManager = LauncherErrorManager.Instance;
            VRErrorManager vrErrorManager = VRErrorManager.Instance;

            try
            {
                assetInfo = json.JsonDeserialize<AssetInfo>();
                
                if (assetInfo.AssetName != null)
                {
                    return true;
                }

                string message = $"Asset name can not be null. {o.Config.i18n.en} Bundle.json is not actual version!";
                LogManager.GetCurrentClassLogger().Fatal(message);

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", "null asset");
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", "null asset");
                }

                return false;
            }
            catch (Exception e)
            {
                string message = $"AssetInfo can not be loaded. {o.Config.i18n.en} Bundle.json is not actual version! Bundle.json = {json}";
                LogManager.GetCurrentClassLogger().Fatal(message);

                if (launcherErrorManager != null)
                {
                    launcherErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", e.StackTrace);
                }

                if (vrErrorManager != null)
                {
                    vrErrorManager.ShowFatal($"{o.Config.i18n.en} not actual!", e.StackTrace);
                }
                 
                return false;
            }
        }

        public static void LoadResourcesApi(AssetInfo assetInfo, PrefabObject o)
        {
            string assetBundleUri = o.Resources.Bundle;

            if (assetInfo.Resources == null)
            {
                return;
            }

            foreach (string assetName in assetInfo.Resources)
            {
                new RequestAsset(assetName, assetBundleUri).OnFinish += response2 =>
                {
                    ResponseAsset responseResource = (ResponseAsset) response2;
                    ObjectResourses.AddResourse(responseResource.Asset);
                };
            }
        }

        public static void LoadResourcesTar(AssetInfo assetInfo, PrefabObject o)
        {
            string assetBundleUri = o.Resources.Bundle;

            if (assetInfo.Resources != null)
            {
                foreach (string resource in assetInfo.Resources)
                {
                    new RequestTar(assetBundleUri).OnFinish += response =>
                    {
                        var bundle = ((ResponseTar) response).ByteData;

                        new RequestLoadAssetFromMemory(resource, bundle).OnFinish += response2 =>
                        {
                            ResponseAsset responseResource = (ResponseAsset) response2;
                            ObjectResourses.AddResourse(responseResource.Asset);
                        };
                    };
                }
            }
        }

        public static void CreatePrefabApi(AssetInfo assetInfo, PrefabObject o, ref GameEntity loadCounter)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = o.Resources.Bundle;

            RequestAsset requestAsset =
                new RequestAsset(assetName, assetBundleUri, new object[] {o, assetInfo});
            GameEntity counter = loadCounter;

            requestAsset.OnFinish += response => { CreatePrefabEntity(response, ref counter, o); };
            //ToDo OnError
        }

        private static void CreatePrefabEntity(IResponse response, ref GameEntity counter, PrefabObject o, Sprite icon = null)
        {
            ResponseAsset responseAsset = (ResponseAsset) response;
            Object unityObject = responseAsset.Asset;
            PrefabObject serverObject = (PrefabObject) responseAsset.UserData[0];
            GameEntity entity = Contexts.sharedInstance.game.CreateEntity();
            entity.AddServerObject(serverObject);
            GameStateData.AddPrefabGameObject(serverObject.Id, responseAsset.Asset, o);
            GameStateData.AddObjectIcon(serverObject.Id, icon);

            if (o.Embedded)
            {
                GameStateData.AddToEmbeddedList(serverObject.Id);
            }
            
            AddCastComponent(unityObject, entity);
            entity.AddIcon(icon);
            counter.loadObjectsCounter.PrefabsLoaded++;
            LogManager.GetCurrentClassLogger().Info(o.Config.i18n.en + " is loaded");
        }

        public static void CreatePrefabTar(AssetInfo assetInfo, PrefabObject o, ref GameEntity loadCounter)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = o.Resources.Bundle;


            GameEntity gameEntity = loadCounter;
            RequestTar requestBundle = new RequestTar(assetBundleUri);

            requestBundle.OnFinish += response =>
            {
                var bundle = ((ResponseTar) response).ByteData;

                RequestLoadAssetFromMemory requestAsset =
                    new RequestLoadAssetFromMemory(assetName, bundle, new object[] {o, assetInfo});
                GameEntity counter = gameEntity;

                requestAsset.OnFinish += response1 => { CreatePrefabEntity(response1, ref counter, o); };

                //ToDo OnError
            };

            //ToDo OnError
        }

        public static void ShowErrorLoadObject(PrefabObject o, string error)
        {
            string message = $"Load object {o.Config.i18n.en} error! {error}";
            LogManager.GetCurrentClassLogger().Fatal(message);
            LauncherErrorManager.Instance.ShowFatal(message, null);
        }

        public static void ShowErrorLoadScene()
        {
            string message = ErrorHelper.GetErrorDescByCode(TM.Errors.ErrorCode.LoadSceneError);
            LogManager.GetCurrentClassLogger().Fatal("Location is not loaded");
            LauncherErrorManager.Instance.Show(message, null);
        }

        public static void LoadCustomAssetTar(AssetInfo assetInfo, PrefabObject o, ref GameEntity loadCounter, Dictionary<string, Type> typesInDll)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = o.Resources.Bundle;

            RequestTar requestAssetData = new RequestTar(assetBundleUri);
            GameEntity entity = loadCounter;

            requestAssetData.OnFinish += responseData =>
            {
                ResponseTar responseTar = (ResponseTar) responseData;
                var bundleData = responseTar.ByteData;

                RequestLoadAssetFromMemory requestAsset =
                    new RequestLoadAssetFromMemory(assetName, bundleData, new object[] {o, assetInfo});
                GameEntity counter = entity;

                requestAsset.OnFinish += response =>
                {
                    CreatePrefabEntity(response, ref counter, o);
                };
            };
        }

        public static void LoadCustomAssetApi(AssetInfo assetInfo, PrefabObject o, ref GameEntity loadCounter, Dictionary<string, Type> typesInDll)
        {
            string assetName = assetInfo.AssetName;
            string assetBundleUri = o.Resources.Bundle;

            RequestAsset requestAsset =
                new RequestAsset(assetName, assetBundleUri, new object[] {o, assetInfo});
            GameEntity counter = loadCounter;


            requestAsset.OnFinish += responseAsset =>
            {
                RequestTexture requestDownLoad = new RequestTexture(Settings.Instance().ApiHost + o.Resources.Icon, 128, 128, TextureFormat.DXT1);

                requestDownLoad.OnFinish += responseUri =>
                {

                    ResponseTexture responseTexture = (ResponseTexture) responseUri;
                    Texture2D texture2D = responseTexture.Texture;

                    Sprite sprite = Sprite.Create(texture2D, new Rect(0, 0, 128, 128), Vector2.zero);
                    CreatePrefabEntity(responseAsset, ref counter, o, sprite);
                };

                requestDownLoad.OnError += s => { CreatePrefabEntity(responseAsset, ref counter, o); };
            };

        }

        public static bool HaveSaveTransform(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();

            return type.ImplementsInterface<ISaveTransformAware>();
        }

        public static bool HaveInputs(MonoBehaviour behaviour)
        {
            Type type = behaviour.GetType();

            if (type.ImplementsInterface<IUseStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IUseEndAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IGrabStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IGrabEndAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerClickAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerInAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IPointerOutAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<IHighlightAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<ITouchStartAware>())
            {
                return true;
            }

            if (type.ImplementsInterface<ITouchEndAware>())
            {
                return true;
            }

            return false;
        }


        /// <summary>
        ///     Initialize object in platform
        /// </summary>
        /// <param name="idObject">Object type id. Used for save.</param>
        /// <param name="spawnInitParams">Parameters for spawn</param>
        /// <param name="spawnedGameObject">Game object for init</param>
        public static void InitObject(int idObject, SpawnInitParams spawnInitParams, GameObject spawnedGameObject, Config config)
        {
            //var photonView = AddPhoton(spawnedGameObject, spawnInitParams.spawnAsset.IdPhoton);
            GameObject gameObjectLink = spawnedGameObject;
            int idLocation = spawnInitParams.IdLocation;
            int idServer = spawnInitParams.IdServer;
            int idInstance = spawnInitParams.IdInstance;
            string name = spawnInitParams.Name;
            var parentId = spawnInitParams.ParentId;
            
            ObjectController parent = null;

            if (parentId != null)
            {
                parent = GameStateData.GetObjectInLocation(parentId.Value);
            }

            WrappersCollection wrappersCollection = null;

            if (idLocation != 0)
            {
                wrappersCollection = GameStateData.GetWrapperCollection();
            }

            InitObjectParams initObjectParams = new InitObjectParams
            {
                Id = idInstance,
                IdObject = idObject,
                IdLocation = idLocation,
                IdServer = idServer,
                Asset = gameObjectLink,
                Name = name,
                RootGameObject = spawnedGameObject,
                WrappersCollection = wrappersCollection,
                Parent = parent,
                Config = config
            };

            var newController = new ObjectController(initObjectParams);
            
            ObjectControllerCreated?.Invoke(newController);
        }

    }
}
