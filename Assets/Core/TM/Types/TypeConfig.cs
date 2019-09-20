using System;
using TM.Data;

namespace TM.Types
{
    [Obsolete("Will be removed")]
    public class TypeConfig: IJsonSerializable
    {
        /// <summary>
        /// GameObject Name
        /// </summary>
        public string GameObjectName;

        /// <summary>
        /// Object Data
        /// </summary>
        public IObjectData Data;

        /// <summary>
        /// Object Type
        /// </summary>
        public ObjectController Type;

    }

    public class AssemblyInfo
    {
        public string GameObjectName;
        public string AssemblyName;
    }

    public interface IObjectData
    {

    }

}