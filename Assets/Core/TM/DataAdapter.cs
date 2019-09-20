using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TM.Errors;
using NLog;
using TM.ECS.Systems.Loader;
using SmartLocalization;
using UnityEngine;
using UnityEngine.UI;
using TM.Data;
using TM.Data.AssetBundle;
using TM.Data.ServerData;
using TM.UI;
using TM.WWW;
using TMPro;

namespace TM
{
    public static class DataAdapter
    {
        private static GameEntity _loadCounter;

        public delegate void ProgressUpdate(float val);
        public static ProgressUpdate OnLoadingUpdate;
        
        public static void ResetObjectsCounter(int count = -1)
        {
            if (_loadCounter != null && _loadCounter.hasLoadObjectsCounter)
            {
                _loadCounter.ReplaceLoadObjectsCounter(count, 0, false);
            }
            else
            {
                _loadCounter = Contexts.sharedInstance.game.CreateEntity();
                _loadCounter.AddLoadObjectsCounter(count, 0, false);
            }
        }

        public static void LoadObjects()
        {
            LogManager.GetCurrentClassLogger().Info($"<Color=Olive><b>" + "Load Objects started..." + "</b></Color>");
            ResetObjectsCounter();
            WorldData.ObjectsAreLoaded = false;

            ObjectResourses.ClearResourses();
            LoadPrefabObjects(WorldData.WorldStructure.Objects);
        }

        public static void LoadPrefabObjects(List<PrefabObject> objects)
        {
            ResetObjectsCounter(objects.Count);
            WorldData.ObjectsAreLoaded = false;

            for (int i=objects.Count-1; i>=0 ; i--)
            {
                PrefabObject o = objects[i];
                if (o != null)
                {
                    try
                    {
                        LoadPrefabObject(o);
                    }
                    catch (Exception ex)
                    {
                        LauncherErrorManager.Instance.ShowFatal(ErrorHelper.GetErrorDescByCode(TM.Errors.ErrorCode.LoadObjectError) + " " + o.Config.i18n.LocalizedString(),
                            ex.StackTrace);
                    }
                }
                else
                {
                    WorldData.WorldStructure.Objects.RemoveAt(i);
                    _loadCounter.ReplaceLoadObjectsCounter(i-1, 0, false);
                }
            }
        }

        private static void LoadPrefabObject(PrefabObject o)
        {
            string dllCachePath = Path.Combine(Application.dataPath, "/cache/dll/" + o.Config.i18n.en + o.Guid);
            Dictionary<string, Type> typesInDll = new Dictionary<string, Type>();

            RequestTar request = new RequestTar(o.Resources.Config);

            request.OnFinish += response =>
            {
                string json = ((ResponseTar) response).TextData;
                AssetInfo assetInfo = null;

                if (!Helper.LoadAssetInfo(json,
                    ref assetInfo,
                    ref _loadCounter,
                    o))
                {
                    return;
                }

                Helper.LoadResourcesTar(assetInfo, o);

                foreach (string dllName in assetInfo.Assembly)
                {
                    string dllPath = o.Resources.DllPath + "/" + dllName;

                    new RequestTar(dllPath).OnFinish += response1 =>
                    {
                        var byteData = ((ResponseTar) response1).ByteData;

                        LoadAssemby(dllCachePath,
                            dllName,
                            ref byteData,
                            ref typesInDll);
                    };
                }

                Helper.LoadCustomAssetTar(assetInfo,
                    o,
                    ref _loadCounter,
                    typesInDll);
            };

            request.OnError += s => { Helper.ShowErrorLoadObject(o, s); };
        }

        private static void LoadAssemby(string dllPath, string dllName, ref byte[] byteData,
            ref Dictionary<string, Type> typesInDll)
        {
            if (!Directory.Exists(dllPath))
            {
                Directory.CreateDirectory(dllPath);
            }

            string dllFileName = dllPath + "/" + dllName;
            File.WriteAllBytes(dllFileName, byteData);
            Assembly assembly = Assembly.Load(byteData);
            GameStateData.AddAssembly(assembly.ManifestModule.Name, dllFileName);

            foreach (Type exportedType in assembly.GetExportedTypes())
            {
                Debug.Log(exportedType + " is loaded!");

                if (exportedType.FullName != null)
                {
                    typesInDll.Add(exportedType.FullName, exportedType);
                }
            }
        }


        private static DateTime _startLoadingTime;

        public static void LoadScene(int locationId)
        {
            _startLoadingTime = DateTime.Now;

            LocationPrefub location = WorldData.WorldStructure.Locations.GetLocation(locationId);

            if (location == null)
            {

                return;
            }

            string sceneName = location.Name;

            LogManager.GetCurrentClassLogger().Info($"Loading location \"{sceneName}\" from tar file");
            RequestTar requestConfig = new RequestTar(location.Resources.Config);

            requestConfig.OnFinish += responseConfig =>
            {
                SceneData sceneData = ((ResponseTar) responseConfig).TextData.JsonDeserialize<SceneData>();
                RequestTar requestTar = new RequestTar(location.Resources.Bundle);

                requestTar.OnFinish += response =>
                {
                    ResponseTar responseTar = (ResponseTar) response;
                    byte[] bundle = responseTar.ByteData;

                    RequestLoadSceneFromMemory request =
                        new RequestLoadSceneFromMemory(sceneData.AssetBundleLabel, bundle);

                    request.OnFinish += response1 =>
                    {
                        ResponseAsset responseAsset = (ResponseAsset) response1;
                        string scenePath = Path.GetFileNameWithoutExtension(responseAsset.Path);

                        WorldDataListener.Instance.LoadScene(scenePath);
                    };

                    Resources.UnloadUnusedAssets();

                    request.OnError += s => { Helper.ShowErrorLoadScene(); };
                };

                requestTar.OnError += s => { Helper.ShowErrorLoadScene(); };
            };

        }

        private static bool Equaller(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            return !a.Where((t, i) => t != b[i]).Any();
        }
        
        private static void LoadSceneFromStorage(LocationPrefub locationPrefub, string environmentDirectory)
        {
            string sceneName = locationPrefub.Name;
            LogManager.GetCurrentClassLogger().Info($"Loading location \"{sceneName}\" from storage: " + Time.time);
            byte[] bytes = null;

            RequestTread requestTread = new RequestTread(delegate
            {
                bytes = File.ReadAllBytes(Path.Combine(environmentDirectory, "bundle"));
            });

            requestTread.OnFinish += responseTread =>
            {
                RequestLoadSceneFromMemory request = new RequestLoadSceneFromMemory(sceneName, bytes);

                request.OnFinish += response1 =>
                {
                    ResponseAsset response = (ResponseAsset) response1;
                    string scenePath = Path.GetFileNameWithoutExtension(response.Path);
                    WorldDataListener.Instance.LoadScene(scenePath);
                };

                request.OnError += s =>
                {
                    string message = ErrorHelper.GetErrorDescByCode(TM.Errors.ErrorCode.EnvironmentNotFoundError);
                    LogManager.GetCurrentClassLogger().Fatal("Location is not loaded from storage! " + s);
                    LauncherErrorManager.Instance.Show(message, null);
                };
            };
        }
    }

}
