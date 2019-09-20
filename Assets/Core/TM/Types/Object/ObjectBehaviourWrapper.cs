using NLog;
using UnityEngine;

namespace TM
{
    public class ObjectBehaviourWrapper : MonoBehaviour
    {
        [SerializeField]
        public ObjectController OwdObjectController;

        public void OnClick()
        {
            LogManager.GetCurrentClassLogger().Info($"Pointer click to {OwdObjectController.Name}");
            ParentManager.Instance.Invoke(OwdObjectController);
        }

       
    }
}
