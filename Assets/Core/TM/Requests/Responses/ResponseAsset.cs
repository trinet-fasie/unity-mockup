using TM.WWW;

namespace TM.Data.ServerData
{
    public class ResponseAsset : IResponse
    {
        public UnityEngine.Object Asset;
        public string Path;
        public object[] UserData;
    }
}
