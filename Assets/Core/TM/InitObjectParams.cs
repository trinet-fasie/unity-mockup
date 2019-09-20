using TM.Types;
using UnityEngine;

namespace TM
{
    public class InitObjectParams
    {
        /// <summary>
        /// Instance id in Wrapper Collection
        /// </summary>
        public int Id;
        /// <summary>
        /// Id group
        /// </summary>
        public int IdLocation;
        /// <summary>
        /// Object Type Id. Used to save.
        /// </summary>
        public int IdObject;
        /// <summary>
        /// Instance id from server api
        /// </summary>
        public int IdServer;
        public string Name;
        public GameObject RootGameObject;
        public GameObject Asset;
        public WrappersCollection WrappersCollection;
        public ObjectController Parent;
        public Config Config;
    }
}
