using TM.WWW;

namespace TM.Data
{
    public class ResponseTar : IResponse
    {
        public object[] UserData;
        public byte[] ByteData;
        public string TextData => System.Text.Encoding.Default.GetString(ByteData);
    }
}
