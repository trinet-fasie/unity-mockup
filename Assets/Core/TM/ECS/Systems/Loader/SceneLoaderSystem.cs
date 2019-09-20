using Entitas;
using NLog;
using TMPro;
using UnityEngine.UI;


namespace TM.ECS.Systems.Loader
{
    public sealed class SceneLoaderSystem : IInitializeSystem
    {
        private readonly Contexts _contexts;
        private readonly int _locationId;
        private readonly Logger _logger;

        public SceneLoaderSystem(Contexts context, TMP_Text debugText, int locationId)
        {
            _contexts = context;
            
            _logger = LogManager.GetCurrentClassLogger();
            _locationId = locationId;
        }

        public void Initialize()
        {
            _logger.Info("Scene load started");
            DataAdapter.LoadScene(_locationId);
        }

       
    }
}