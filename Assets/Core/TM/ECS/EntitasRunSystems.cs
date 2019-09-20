using UnityEngine;
using TM.ECS.Systems;

namespace TM.ECS
{
    public class EntitasRunSystems : MonoBehaviour
    {
        private GameSystems _systems;
        private void Start()
        {
            _systems = new GameSystems(Contexts.sharedInstance);
            _systems.Initialize();
        }

        private void Update()
        {
            _systems.Execute();
            _systems.Cleanup();
        }
    }
}
