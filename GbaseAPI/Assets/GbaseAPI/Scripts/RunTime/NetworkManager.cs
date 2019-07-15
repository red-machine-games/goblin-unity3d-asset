using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Gbase
{
    public class NetworkManager : MonoBehaviour
    {
        private int _requestSequence = -1;
        private Queue<KeyValuePair<UnityWebRequest, Action<GbaseError, string>>> _requestsQueue =
            new Queue<KeyValuePair<UnityWebRequest, Action<GbaseError, string>>>();

        private bool _canPing;
        private string _unicorn;
        public string Unicorn
        {
            set
            {
                if (string.IsNullOrEmpty(_unicorn))
                {
                    _canPing = true;
                    _unicorn = value;
                }
            }
        }

        private string _url;
        private string _hmacSecret;
        private string _platformVersion;
        private bool _processingRequests;
        private float _pingDt;
        private float _noInputDt;
        private int _repeatCount;
        
        private const int MAX_REPEAT_COUNT = 3;
        private const int TIMEOUT_SECONDS = 15;
        private const int PING_DELTA_TIME = 45;
        private const int NO_INPUT_DELTA_TIME = 180;
        private const int SERVER_UNDER_MAINTENCE_DELAY = 10;

        private void Update()
        {
            if (_canPing)
            {
                if (Input.anyKeyDown)
                {
                    _pingDt = PING_DELTA_TIME;
                    _noInputDt = NO_INPUT_DELTA_TIME;
                }
                else
                {
                    var dt = Time.deltaTime;
                    _pingDt -= dt;
                    _noInputDt -= dt;
                    _canPing = _noInputDt > 0;

                    if (_canPing && _pingDt < 0)
                    {
                        _pingDt = PING_DELTA_TIME;
                        var route = _url + "api/v0/utils.ping";
                        var request = new Request(RequestMethod.GET, route);
                        PingServer(request.www);
                    }
                }
            }
            if (_requestsQueue.Count > 0 && !_processingRequests)
            {
                StartCoroutine(ProcessRequestsQueue());
            }
        }
        
        public void Init(string url, string hmacSecret, string platform, string version)
        {
            _url = url;
            _hmacSecret = hmacSecret;
            _platformVersion = platform + ";" + version;
            _pingDt = PING_DELTA_TIME;
            _noInputDt = NO_INPUT_DELTA_TIME;
        }
        
        public void Reset()
        {
            StopAllCoroutines();
            _requestSequence = -1;
            _requestsQueue.Clear();
            _canPing = false;
            _pingDt = PING_DELTA_TIME;
            _noInputDt = NO_INPUT_DELTA_TIME;
            _unicorn = null;
            _processingRequests = false;
            _repeatCount = 0;
        }

        public void SendTestRequest(UnityWebRequest request, Action<GbaseError, string> callback)
        {
            request.timeout = TIMEOUT_SECONDS;
            _requestsQueue.Enqueue(new KeyValuePair<UnityWebRequest, Action<GbaseError, string>>(request, callback));
            StartCoroutine(SendRequestCoroutine(request, callback));
        }
        
        public void SendRequest(UnityWebRequest request, Action<GbaseError, string> callback)
        {
            request.timeout = TIMEOUT_SECONDS;
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            request.SetRequestHeader("X-Req-Seq", (++_requestSequence).ToString());
            request.SetRequestHeader("X-Platform-Version", _platformVersion);
            request.SetRequestHeader("X-Request-Sign", GetHmacSign(request));
            if (!string.IsNullOrEmpty(_unicorn))
            {
                request.SetRequestHeader("X-Unicorn", _unicorn);
            }

            _requestsQueue.Enqueue(new KeyValuePair<UnityWebRequest, Action<GbaseError, string>>(request, callback));
        }
        
        public void PingServer(UnityWebRequest request)
        {
            request.timeout = TIMEOUT_SECONDS;
            request.SetRequestHeader("X-Platform-Version", _platformVersion);
            if (!string.IsNullOrEmpty(_unicorn))
            {
                request.SetRequestHeader("X-Unicorn", _unicorn);
            }

            StartCoroutine(SendRequestCoroutine(request, null, false));
        }

        private string GetHmacSign(UnityWebRequest request)
        {
            var sign = new StringBuilder();
            var uri = new Uri(request.url);
            sign.Append(uri.PathAndQuery);
            if (request.method == "POST")
            {
                sign.Append(Encoding.UTF8.GetString(request.uploadHandler.data));
            }
            sign.Append(_requestSequence);
            if (!string.IsNullOrEmpty(_unicorn))
            {
                sign.Append(_unicorn);
            }
            sign.Append(_hmacSecret);
            
            var bytes = Encoding.UTF8.GetBytes(sign.ToString());
            var hashString = new SHA256Managed();
            var hash = hashString.ComputeHash(bytes);
            var signHash = string.Empty;
            
            foreach (var b in hash)
            {
                signHash += string.Format("{0:x2}", b);
            }

            return signHash;
        }

        private IEnumerator ProcessRequestsQueue()
        {
            _processingRequests = true;

            while (_requestsQueue.Count > 0)
            {
                var requestAndCallback = _requestsQueue.Peek();
                yield return StartCoroutine(SendRequestCoroutine(requestAndCallback.Key, requestAndCallback.Value));
                
                if (_noInputDt < 0)
                {
                    _pingDt = PING_DELTA_TIME;
                    _noInputDt = NO_INPUT_DELTA_TIME;
                    _canPing = true;
                }
            }

            _processingRequests = false;
        }

        private IEnumerator SendRequestCoroutine(UnityWebRequest request, Action<GbaseError, string> callback, bool dequeue = true)
        {
            GbaseAPI.Print("~~> " + request.method + " " + request.url);
            yield return request.SendWebRequest();
            GbaseAPI.Print("<~~ " + request.method + " " + request.responseCode + " " + request.downloadHandler.text);

            if (request.responseCode == 0)
            {
                yield return StartCoroutine(RepeatLastRequest(request, callback));
                if (dequeue)
                {
                    _requestsQueue.Dequeue();
                }
            }
            else
            {
                if (dequeue)
                {
                    _requestsQueue.Dequeue();
                }
                if (request.isHttpError || request.isNetworkError)
                {
                    if (IsDeadSession(request))
                    {
                        GbaseAPI.Instance.Reauth(() =>
                        {
                            var lastRequest = new Request(request.method, request.url, 
                                request.uploadHandler != null ? request.uploadHandler.data : new byte[0]);
                            SendRequest(lastRequest.www, callback);
                        });
                    }
                    else if (IsUnderMaintence(request))
                    {
                        yield return StartCoroutine(RepeatLastRequest(request, callback, SERVER_UNDER_MAINTENCE_DELAY));
                    }
                    else
                    {
                        if (callback != null)
                        {
                            callback(new GbaseError(request.downloadHandler.text), null);
                        }
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(null, request.downloadHandler.text);
                    }
                }
            }
        }

        private IEnumerator RepeatLastRequest(UnityWebRequest req, Action<GbaseError, string> callback, float delay = -1)
        {
            if (delay > 0)
            {
                yield return new WaitForSeconds(delay);
            }
            
            var request = new Request(req.method, req.url, req.uploadHandler.data).www;
            
            request.timeout = TIMEOUT_SECONDS;
            request.SetRequestHeader("Content-Type", "application/json; charset=UTF-8");
            request.SetRequestHeader("X-Req-Seq", req.GetRequestHeader("X-Req-Seq"));
            request.SetRequestHeader("X-Platform-Version", _platformVersion);
            request.SetRequestHeader("X-Request-Sign", GetHmacSign(request));
            if (!string.IsNullOrEmpty(_unicorn))
            {
                request.SetRequestHeader("X-Unicorn", _unicorn);
            }
            
            GbaseAPI.Print("~~> " + request.method + " " + request.url);
            yield return request.SendWebRequest();
            GbaseAPI.Print("<~~ " + request.method + " " + request.responseCode + " " + request.downloadHandler.text);

            if (request.responseCode == 0)
            {
                if (++_repeatCount >= MAX_REPEAT_COUNT)
                {
                    GbaseAPI.Instance.Quit();
                    if (callback != null)
                    {
                        callback(new GbaseError(0, "Connection problem"), null);
                    }
                }
                else
                {
                    yield return StartCoroutine(RepeatLastRequest(request, callback));
                }
            }
            else
            {
                if (request.isHttpError || request.isNetworkError)
                {
                    if (IsSequenceMismatch(request))
                    {
                        if (callback != null)
                        {
                            callback(null, "OK");
                        }
                    }
                    else if (IsUnderMaintence(request))
                    {
                        yield return StartCoroutine(RepeatLastRequest(request, callback, SERVER_UNDER_MAINTENCE_DELAY));
                    }
                    else
                    {
                        if (callback != null)
                        {
                            callback(new GbaseError(request.downloadHandler.text), null);
                        }
                    }
                }
                else
                {
                    if (callback != null)
                    {
                        callback(null, request.downloadHandler.text);
                    }
                }
            }
        }
        
        private bool IsSequenceMismatch(UnityWebRequest request)
        {
            return request.responseCode != 200 && request.downloadHandler.text.Contains("HMAC: sequence mismatch");
        }
        
        private bool IsDeadSession(UnityWebRequest request)
        {
            return request.responseCode != 200 && request.downloadHandler.text.Contains("Unknown unicorn");
        }
        
        private bool IsUnderMaintence(UnityWebRequest request)
        {
            return request.responseCode != 200 && (request.downloadHandler.text.Contains("Under maintenance now") ||
                                                   request.downloadHandler.text.Contains("Under compelled maintenance now"));
        }
    }
}