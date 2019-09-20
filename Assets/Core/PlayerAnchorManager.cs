using System.Collections;
using NLog;
using TM;
using UnityEngine;

//This version is for usage inside OpW
public class PlayerAnchorManager : MonoBehaviour
{
    private bool _trackingInitialized;
    private Transform _headTransform;
    private GameObject _player;

    private void Awake()
    {
        if (FindObjectOfType<GameObjects>() != null)
        {
            return;
        }
        
        GameObject opwPlayerRig = Resources.Load<GameObject>("OpW Player Rig");

        if (opwPlayerRig == null)
        {
            return;
        }

        
            _player = Instantiate(opwPlayerRig, transform.position, Quaternion.identity);
            _headTransform = _player.transform.Find("[VRTK_SDKManager]").Find("SDKSetups").Find("SteamVR").GetComponentInChildren<Camera>().transform;
            _trackingInitialized = false;

            StartCoroutine(InvokeOnPlayerInitialization());

    }

    private void Start()
    {
        StartCoroutine(InvokeOnLoadLocation());
    }

    private IEnumerator RestartPlayer()
    {
        _player.SetActive(false);
        yield return new WaitForEndOfFrame();
        _player.SetActive(true);
        yield return true;
    }

    IEnumerator InvokeOnLoadLocation()
    {
        while (!WorldData.ObjectsAreLoaded)
        {
            yield return null;
        }

        yield return new WaitForEndOfFrame();

        LogManager.GetCurrentClassLogger().Info("WorldData.OnLoadLocation?.Invoke();");

        WorldData.OnLoadLocation?.Invoke();
        yield return true;
    }

    private IEnumerator InvokeOnPlayerInitialization()
    {
        while (!_trackingInitialized)
        {
            Vector3 diff = _headTransform.position - _player.transform.position;

            if (diff != Vector3.zero) 
            {             
                _trackingInitialized = true;

                Vector3 newRot = _player.transform.eulerAngles;

                newRot.x = 0;
                newRot.y -= _headTransform.transform.eulerAngles.y - transform.eulerAngles.y;
                newRot.z = 0;

                _player.transform.rotation = Quaternion.Euler(newRot);

                Vector3 newPos = _player.transform.position;
                diff = _headTransform.position - newPos;
                newPos.x -= diff.x;
                newPos.z -= diff.z;

                _player.transform.position = newPos;

                LogManager.GetCurrentClassLogger().Info("Player position initialized");
            }

            yield return null;
        }

        yield return true;
    }
    
    public void ReloadPlayer(GameObject newPlayer)
    {
        _player = newPlayer;
        _player.transform.position = transform.position;
        _headTransform = _player.transform.GetComponentInChildren<Camera>().transform;
        _trackingInitialized = false;
        StartCoroutine(InvokeOnPlayerInitialization());
        StartCoroutine(RestartPlayer());
    }
}