using TM.Errors;
using NLogger;
using TM;
using TM.Data;
using TM.ECS.Systems.Loader;
using UnityEngine;
using TM.Public;

public class DebugLauncher : MonoBehaviour
{
    public GameObject PlayerRig;
    public Transform PlayerSpawnPoint;
    public GameMode GameMode;

    private void Update()
    {
        if (WorldData.GameMode != GameMode)
            WorldData.GameMode = GameMode;
    }

    private void Start()
    {
        if (PlayerRig == null || PlayerSpawnPoint == null)
        {
            Debug.LogError("PlayerRig or PlayerSpawnPoint is null");
            return;
        }

        NLoggerSettings.Init();
        Application.logMessageReceived += ErrorHelper.ErrorHandler;
        Settings.GreateDebugSettings("");

        var playerRig = Instantiate(PlayerRig);
        playerRig.transform.position = PlayerSpawnPoint.position;

        WorldData.GameMode = GameMode;
        InitObjectsOnScene();
    }

    private void InitObjectsOnScene()
    {
        var owdObjects = FindObjectsOfType<TMObjectDescriptor>();

        foreach (var owdObject in owdObjects)
        {
            SpawnInitParams spawn = new SpawnInitParams
            {
                Name = owdObject.Name,
                IdLocation = 1,
                IdInstance = 0,
                IdObject = 0,
                IdServer = 0
            };

            Helper.InitObject(0, spawn, owdObject.gameObject, null);
        }
    }

     
}
