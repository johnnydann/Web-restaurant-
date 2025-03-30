using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
//using Newtonsoft.Json;

namespace ChieuT4_Nhom05_WebQLCF.Helper
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key,
        object value)
        {
            session.SetString(key, JsonConvert.SerializeObject(value));
        }
        public static T GetObjectFromJson<T>(this ISession session, string
        key)
        {
            var value = session.GetString(key);
            return value == null ? default :
            JsonConvert.DeserializeObject<T>(value);
        }
    }
}
