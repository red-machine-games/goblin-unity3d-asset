using UnityEngine.Networking;
using System.Text;

namespace Gbase
{
    public enum RequestMethod
    {
        GET = 0,
        POST = 1
    }
    
    public class Request
    {
        public readonly UnityWebRequest www;

        public Request(RequestMethod method, string url, string body = "")
        {
            if (method == RequestMethod.GET)
            {
                www = UnityWebRequest.Get(url);
            }
            else
            {
                if (string.IsNullOrEmpty(body))
                {
                    www = UnityWebRequest.Post(url, body);
                }
                else
                {
                    www = UnityWebRequest.Put(url, Encoding.UTF8.GetBytes(body));
                    www.method = "POST";
                }
            }
        }
        
        public Request(string method, string url, byte[] bodyData)
        {
            if (bodyData.Length == 0)
            {
                www = UnityWebRequest.Post(url, "");
            }
            else
            {
                www = UnityWebRequest.Put(url, bodyData);
            }

            www.method = method;
        }
    }
}