using UnityEngine;
using TM.Data;

namespace TM.New.PUN
{
    class Spawner : MonoBehaviour
    {
        public static Spawner Instance;

        private void Start()
        {
            Instance = this;
        }
 
        public void SpawnAsset(SpawnInitParams param)
        {
            SpawnAsset(param, 0);
        }

        public void SpawnAsset(SpawnInitParams paramObject, int idPhoton)
        {
            Contexts contexts = Contexts.sharedInstance;
            var entity = contexts.game.CreateEntity();
            entity.AddSpawnAsset(paramObject, idPhoton);
        }

        public void SpawnAssetRpc(string paramObject, int idPhoton)
        {
            SpawnInitParams param = paramObject.JsonDeserialize<SpawnInitParams>(); 
            Contexts contexts = Contexts.sharedInstance;
            var entity = contexts.game.CreateEntity();
            entity.AddSpawnAsset(param, idPhoton);
        }
    }

   
}
