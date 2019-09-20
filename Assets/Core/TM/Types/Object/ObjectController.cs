using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DesperateDevs.Utils;
using Entitas.VisualDebugging.Unity;
using Newtonsoft.Json;
using NLog;
using UnityEngine;
using TM.Data;
using TM.Data.ServerData;
using TM.Models.Data;
using TM.Public;
using TM.WWW;
using VRTK;
using Object = UnityEngine.Object;

#pragma warning disable 618

namespace TM
{
    /// <summary>
    /// Base class for all objects
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public sealed class ObjectController
    {
        #region PRIVATE VARS

        private Contexts _context;
        private int _parentId;
        private ObjectController _parent;
        private Config _config;
        private readonly Dictionary<int, InputController> _inputControllers = new Dictionary<int, InputController>();
        private readonly Dictionary<Rigidbody, bool> _rigidBodyKinematicsDefaults = new Dictionary<Rigidbody, bool>();

        private readonly List<GameModeSwitchController> _gameModeSwitchControllers =
            new List<GameModeSwitchController>();

        private readonly List<ColliderController> _colliderControllers = new List<ColliderController>();
        private readonly List<ObjectTransform> _objectTransforms = new List<ObjectTransform>();

        #endregion

        #region PUBLIC VARS

        // ReSharper disable once InconsistentNaming
        /// <summary>
        /// Link to gameObject with IWraperAware
        /// </summary>
        public GameObject gameObject { get; private set; }

        /// <summary>
        /// Link to root gameObject
        /// </summary>
        public GameObject RootGameObject { get; private set; }

        /// <summary>
        /// ECS Entity link
        /// </summary>
        public GameEntity Entity;

        /// <summary>
        /// ECS Entity draw line to parent
        /// </summary>
        public GameEntity EntityParentLine;

        public Action OnDestroy = delegate { };

        /// <summary>
        /// Object Name
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Id inside group
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// Location Id 
        /// </summary>
        public int IdLocation { get; private set; }

        /// <summary>
        /// Id object inside server
        /// </summary>
        public int IdServer { get; private set; }

        /// <summary>
        /// Object Type Id. Used to save.
        /// </summary>
        public int IdObject { get; private set; }

        public WrappersCollection WrappersCollection { get; private set; }

        /// <summary>
        /// Parent BaseType (GameObject)
        /// </summary>
        public ObjectController Parent
        {
            get { return _parent; }
            set { SetParent(value); }
        }

        private void SetParent(ObjectController value)
        {
            _parent = value;
            Vector3 scale = RootGameObject.transform.localScale;

            if (value != null)
            {
                _parentId = value.Id;
                Entity.ReplaceIdParent(_parentId);

                if (EntityParentLine == null)
                {
                    EntityParentLine = _context.game.CreateEntity();
                    GameObject go = new GameObject("LineToParent");
                    EntityParentLine.AddGameObject(go);
                    LineRenderer lineRenderer = Helper.CreateLineRenderer(go);

                    if (lineRenderer != null)
                    {
                        EntityParentLine.AddLineRenderer(lineRenderer);

                        EntityParentLine.AddChildrenParentTransform(RootGameObject.transform,
                            value.RootGameObject.transform);
                    }
                }
                else
                {
                    EntityParentLine.ReplaceChildrenParentTransform(RootGameObject.transform,
                        value.RootGameObject.transform);
                }

                RootGameObject.transform.SetParent(value.RootGameObject.transform, true);

                LogManager.GetCurrentClassLogger()
                    .Info($"New parent {value.RootGameObject.name} was set to {RootGameObject.name}");
            }
            else
            {
                _parentId = 0;
                RootGameObject.transform.SetParent(null, true);

                if (Entity.hasIdParent)
                {
                    Entity.RemoveIdParent();
                }

                if (EntityParentLine != null)
                {
                    if (EntityParentLine.hasGameObject && EntityParentLine.gameObject.Value != null)
                    {
                        Object.Destroy(EntityParentLine.gameObject.Value);
                    }

                    EntityParentLine.Destroy();
                    EntityParentLine = null;
                }

                LogManager.GetCurrentClassLogger().Info($"Parent was remove to {RootGameObject.name}");
            }

            RootGameObject.transform.SetGlobalScale(scale);
        }
        
        #endregion

        #region ABSTRACT METHODS 

        #endregion

        #region PUBLIC METHODS

        public ObjectController(InitObjectParams initObjectParams)
        {
            _context = Contexts.sharedInstance;
            Entity = _context.game.CreateEntity();
            _config = initObjectParams.Config;

            RootGameObject = initObjectParams.RootGameObject;
            gameObject = initObjectParams.Asset;
            Id = initObjectParams.Id;
            IdObject = initObjectParams.IdObject;
            IdLocation = initObjectParams.IdLocation;
            IdServer = initObjectParams.IdServer;
            Name = initObjectParams.Name;
            WrappersCollection = initObjectParams.WrappersCollection;

            int instanceId = Id;
            this.RegisterMeInLocation(ref instanceId);
            Id = instanceId;
            SetName(Name);

            if (initObjectParams.Parent != null)
            {
                SetParent(initObjectParams.Parent);
            }

            RootGameObject.AddComponent<ObjectBehaviourWrapper>().OwdObjectController = this;

            AddBehaviours();
            AddWrapper();
            SaveKinematics();

            Entity.AddId(Id);
            Entity.AddIdServer(IdServer);
            Entity.AddIdObject(IdObject);
            Entity.AddRootGameObject(RootGameObject);
            Entity.AddGameObject(gameObject);

            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();

            if (rigidbody != null)
            {
                Entity.AddRigidbody(rigidbody);
            }

            Collider collider = gameObject.GetComponentInChildren<Collider>();

            if (collider)
            {
                Entity.AddCollider(collider);
            }
           
            ApplyGameMode(WorldData.GameMode, WorldData.GameMode);
            RequestManager.Instance.StartCoroutine(ExecuteSwitchGameModeDelayedCoroutine());

            Create();
            
        }

        public IEnumerator ExecuteSwitchGameModeDelayedCoroutine()
        {
            yield return null;         
            ExecuteSwitchGameModeOnObject(WorldData.GameMode, WorldData.GameMode);       
        }

        private void Create()
        {
            var monobehaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>();

            foreach (MonoBehaviour monoBehaviour in monobehaviours)
            {
                if (monoBehaviour == null)
                {
                    continue;
                }

                MethodInfo method = monoBehaviour.GetType()
                    .GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);

                if (method != null && method.GetParameters().Length == 0)
                {
                    method.Invoke(monoBehaviour, null);
                }
            }
        }

        private void SaveKinematics()
        {
            var rigidBodies = RootGameObject.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rigidbody in rigidBodies)
            {
                _rigidBodyKinematicsDefaults.Add(rigidbody, rigidbody.isKinematic);
            }
        }


        private void AddBehaviours()
        {
            var behaviours = RootGameObject.GetComponentsInChildren<MonoBehaviour>(true);
            bool haveRootInputControl = false;
            bool haveJoints = false;

            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour == null)
                {
                    continue;
                }

                bool haveInputControlInObject;
                bool haveInputControlInRoot;
                AddInputControl(behaviour, out haveInputControlInObject, out haveInputControlInRoot);

                if (haveInputControlInObject)
                {
                    haveRootInputControl = true;
                }

                AddGameModeSwitchControls(behaviour);
                AddCollidersAware(behaviour);
                AddObjectTransforms(behaviour);
            }

            if (!haveRootInputControl)
            {
                var inputController = new InputController(this, RootGameObject,
                    true, true);
                _inputControllers.Add(-1, inputController);
                LogManager.GetCurrentClassLogger().Info("Added input controller on " + RootGameObject.name);
            }

            Entity.AddInputControls(_inputControllers);
        }

        public void AddWrapper()
        {
            IWrapperAware wrapperAware = RootGameObject.GetComponentInChildren<IWrapperAware>();

            if (wrapperAware != null)
            {
                Wrapper wrapper = wrapperAware.Wrapper();
                WrappersCollection.Add(Id, wrapper);
                Entity.AddWrapper(wrapper);
                wrapper.InitEntity(Entity);
            }
        }

        private void AddObjectTransforms(MonoBehaviour child)
        {
            var idComponent = child.gameObject.GetComponent<ObjectId>();

            if (idComponent == null)
            {
                return;
            }

            if (Helper.HaveSaveTransform(child))
            {
                GameObject go = child.gameObject;
                var objectTransform = go.AddComponent<ObjectTransform>();
                _objectTransforms.Add(objectTransform);
                LogManager.GetCurrentClassLogger().Info("Added object transform saver on " + go.name);
            }
        }

        private void AddCollidersAware(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<IColliderAware>())
            {
                return;
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            IColliderAware colliderAware = (IColliderAware) child;
            _colliderControllers.Add(new ColliderController(colliderAware, child.gameObject));
        }

        private void AddGameModeSwitchControls(MonoBehaviour child)
        {
            if (!child.GetType().ImplementsInterface<ISwitchModeSubscriber>())
            {
                return;
            }

            // ReSharper disable once SuspiciousTypeConversion.Global
            ISwitchModeSubscriber switchMode = (ISwitchModeSubscriber) child;
            _gameModeSwitchControllers.Add(new GameModeSwitchController(switchMode));
        }

        public void AddInputControl(MonoBehaviour behaviour, out bool haveInputControlInObject,
            out bool haveInputControlInRoot)
        {
            haveInputControlInObject = false;
            haveInputControlInRoot = false;

            if (behaviour == null)
            {
                return;
            }

            var idComponent = behaviour.gameObject.GetComponent<ObjectId>();

            if (idComponent == null)
            {
                return;
            }

            if (!Helper.HaveInputs(behaviour))
            {
                return;
            }

            int id = idComponent.Id;
            GameObject go = behaviour.gameObject;
            bool root = false;
            bool mainObject = false;

            if (go == RootGameObject)
            {
                haveInputControlInRoot = true;
                root = true;
            }

            if (go == gameObject)
            {
                haveInputControlInObject = true;
                mainObject = true;
            }

            var inputController = _inputControllers.ContainsKey(id)
                ? _inputControllers[id]
                : new InputController(this, go,
                    root, mainObject);

            if (!_inputControllers.ContainsKey(id))
            {
                _inputControllers.Add(id, inputController);
            }

            LogManager.GetCurrentClassLogger().Info("Added input controller on " + go.name);
        }

        public void ApplyGameMode(GameMode newMode, GameMode oldMode)
        {
            foreach (InputController controller in _inputControllers.Values)
            {
                controller.DisableEditorInput();
                controller.ApplyInput();
            }
        }

        public void ExecuteSwitchGameModeOnObject(GameMode newMode, GameMode oldMode)
        {
            foreach (var gameModeSwitchController in _gameModeSwitchControllers)
            {
                try
                {
                    gameModeSwitchController.SwitchGameMode(newMode, oldMode);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Can not apply game mode for {Name}. Error: {e.Message}");
                }
            }
        }

        public Dictionary<int, TransformDT> GetTransforms()
        {
            Dictionary<int, TransformDT> transforms = new Dictionary<int, TransformDT>();

            foreach (var controllersValue in _inputControllers.Values)
            {
                var transform = controllersValue.GetTransform();

                if (transform != null)
                {
                    if (!transforms.Keys.Contains(transform.Id))
                    {
                        transforms.Add(transform.Id, transform.Transform);
                    }
                }
            }

            foreach (ObjectTransform objectTransform in _objectTransforms)
            {
                var transform = objectTransform.GetTransform();

                if (transform != null)
                {
                    if (!transforms.Keys.Contains(transform.Id))
                    {
                        transforms.Add(transform.Id, transform.Transform);
                    }
                }
            }

            return transforms;
        }

 

        public SpawnInitParams GetSpawnInitParams()
        {
            Dictionary<int, TransformDT> transforms = new Dictionary<int, TransformDT>();
            var objectsIds = RootGameObject.GetComponentsInChildren<ObjectId>();

            foreach (var objectId in objectsIds)
            {
                if (!transforms.ContainsKey(objectId.Id))
                {
                    transforms.Add(objectId.Id, objectId.gameObject.transform.ToTransformDT());
                }
            }

            var spawn = new SpawnInitParams
            {
                ParentId = _parentId,
                IdLocation = WorldData.WorldLocationId,
                IdInstance = Id,
                IdObject = IdObject,
                IdServer = IdServer,
                Name = Name,
                Transforms = transforms
            };

            return spawn;
        }

        public void SetServerId(int id)
        {
            IdServer = id;
            Entity.ReplaceIdServer(id);
        }

        public void SetName(string newName)
        {
            Name = newName;
            Entity.ReplaceName(newName);
        }

        public List<ObjectController> GetChildrens()
        {
            List<ObjectController> result = new List<ObjectController>();

            List<ObjectBehaviourWrapper> childrens =
                RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>().ToList();

            foreach (var children in childrens)
            {
                if (children.OwdObjectController.Id == Id)
                {
                    continue;
                }

                result.Add(children.OwdObjectController);
            }

            return result;
        }

        public void Delete()
        {
            List<ObjectBehaviourWrapper> childrens =
                RootGameObject.GetComponentsInChildren<ObjectBehaviourWrapper>().ToList();
            childrens.Reverse();

            foreach (var children in childrens)
            {
                ObjectController type = children.OwdObjectController;
                WrappersCollection.Remove(type.Id);

                foreach (ColliderController colliderController in type._colliderControllers)
                {
                    colliderController.Destroy();
                }

                foreach (InputController inputController in type._inputControllers.Values)
                {
                    inputController.Destroy();
                }

                type.OnDestroy?.Invoke();
                type.Entity?.Destroy();

                if (type.EntityParentLine != null)
                {
                    Object.Destroy(type.EntityParentLine.gameObject.Value);
                    type.EntityParentLine.Destroy();
                }

                type.RootGameObject.DestroyGameObject();
                type.Dispose();
            }

            WorldData.ObjectsAreChanged = true;
        }

        public string GetLocalizedName()
        {
            if (_config == null)
            {
                return "name unknown";
            }

            switch (Settings.Instance().Language)
            {
                case "ru": return _config.i18n.ru;
                case "en": return _config.i18n.en;
            }

            Debug.Log(Settings.Instance().Language);

            return _config.i18n.en;
        }

        #endregion

       
    }
}
