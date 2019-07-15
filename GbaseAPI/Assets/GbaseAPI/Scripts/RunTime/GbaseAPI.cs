using System;
using UnityEditor;
using UnityEngine;

namespace Gbase
{
	enum Platform
	{
		ios = 0,
		android = 1,
		webok = 2,
		stdl = 3,
		webvk = 4,
	}
	
	public class GbaseAPI : MonoBehaviour
	{
		private static GbaseAPI _instance;
		public static GbaseAPI Instance
		{
			get { return _instance; }
		}
		
		[SerializeField] private bool _localhost;
		[SerializeField] private bool _enableLogs;
		
		[SerializeField] private string _hmacSecret = "default";
		[SerializeField] private string _projectName = "project";
		[SerializeField] private string _environment = "dev";
		[SerializeField] private string _domainName = "gbln.app";
		[SerializeField] private Platform _platform = Platform.ios;
		[SerializeField] private string _version = "0.0.1";
		private NetworkManager _networkManager;

		private string _prefix;
		private string _url;
		private Account _account;
		private Profile _profile;
		public Profile Profile
		{
			get { return _profile; }
		}

		private bool _doNotRememberIdSecret;
		private bool _authInProgress;
		private bool _authDone;
		private Action _authCallback;
		private Action _reauthCallback;

		private string _okSecret;
		private string _okId;
		private string _okSessionKey;

		private string _vkId;
		private string _vkSecret;
		private string _vkToken;

		private string _gCustomId;
		private string _gCustomSecret;

		private Action _lastAuth;
		public bool AutoReauth = true;

		private void Awake()
		{
			if (_instance == null)
			{
				_instance = this;
				transform.parent = null;
				DontDestroyOnLoad(gameObject);
				
				_prefix = _localhost ? "local" : string.Format("{0}-{1}.{2}_", _projectName, _environment, _domainName);
				_url = _localhost ? "http://localhost:8000/" : string.Format("https://{0}-{1}.{2}/", _projectName, _environment, _domainName);
				
				if (_networkManager == null)
				{
					_networkManager = gameObject.AddComponent<NetworkManager>();
				}
				_networkManager.Init(_url, _hmacSecret, _platform.ToString(), _version);
			}
			else
			{
				Destroy(gameObject);
			}
		}

		public void SetHmacSecret(string hmacSecret)
		{
			_hmacSecret = hmacSecret;
		}

		public void SetEnvironment(string environment)
		{
			_environment = environment;
		}

		private void AuthCallback(GbaseError error, string response)
		{
			if (error != null)
			{
				Print(error);
				_authInProgress = false;
				_authDone = false;
				throw new Exception(error.message);
			}
			else
			{
				Print(response);
				_account = new Account(response);
				if (!_doNotRememberIdSecret)
				{
					_account.Save(_prefix);
				}

				_networkManager.Unicorn = _account.unicorn;
				GetProfile();
			}
		}
		
		/// <summary>
        /// Этот метод используется для регистрации или входа игрока по известным логину и паролю.
        /// </summary>
        /// <param name="gCustomId">Фактический логин игрока, должен соответствовать регулярному выражению /[A-Za-z0-9._@]{4,32}$/</param>
        /// <param name="gCustomSecret">Фактический пароль игрока, должен соответствовать регулярному выражению /[A-Za-z0-9._@]{6,64}$/</param>
        /// <param name="toSignup">Булевый переключатель, который говорит о том, что нужно зарегистрировать нового игрока при значении true. Или войти существующему - если false</param>
        public void AuthGuestCustom(string gCustomId, string gCustomSecret, bool toSignup, bool doNotRememberIdSecret = false, Action authCallback = null){
        	if (string.IsNullOrEmpty(gCustomId) || string.IsNullOrEmpty(gCustomSecret)){
		        throw new Exception("gCustomId or gCustomSecret is empty");
			}
			if (!_authInProgress)
			{
				_gCustomId = gCustomId;
				_gCustomSecret = gCustomSecret;
				_lastAuth = () => { AuthGuestCustom(_gCustomId, _gCustomSecret, false); };
				_doNotRememberIdSecret = doNotRememberIdSecret;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				string route;
                if(toSignup){
                	route = _url + string.Format("api/v0/accounts.getAccount?gcustomid={0}&gcustomsecret={1}", gCustomId, gCustomSecret);
                } else {
                	route = _url + string.Format("api/v0/accounts.getAccount?gclientid={0}&gclientsecret={1}", gCustomId, gCustomSecret);
                }
				var authRequest = new Request(RequestMethod.POST, route);

				_networkManager.SendRequest(authRequest.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth in progress");
			}
		}

        /// <summary>
		/// Этот метод используется для попытки входа без указания логина и пароля - предпринимается попытка взять логин+пароль из локальной памяти. Если метод бросает соотв. исключение - следует воспользоваться методом входа с явным указанием данных
		/// </summary>
		public void AuthGuest(bool doNotRememberIdSecret = false, Action authCallback = null)
		{
			if (!_authDone && !_authInProgress)
			{
				_lastAuth = () => { AuthGuest(true); };
				_doNotRememberIdSecret = doNotRememberIdSecret;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var savedAccount = Account.GetSavedAccount(_prefix);
				if(savedAccount.gClientId != null && savedAccount.gClientSecret != null){
                	var route = _url + string.Format("api/v0/accounts.getAccount?gclientid={0}&gclientsecret={1}", savedAccount.gClientId, savedAccount.gClientSecret);
                	var authRequest = new Request(RequestMethod.POST, route);

                	_networkManager.SendRequest(authRequest.www, AuthCallback);
                } else {
                	throw new Exception("Does not have locally saved gClientId and gClientSecret! Use AuthGuestCustom or AuthGuestNew instead");
                }
			}
			else
			{
				throw new Exception("Auth already done");
			}
		}

        /// <summary>
        /// Этот метод используется для регистрации нового анонимного игрока. Сервер сам подбирает логин и пароль. Не используйте этот метод для логина - при каждом вызове будет создаваться новый аккаунт!
        /// </summary>
		public void AuthGuestNew(bool doNotRememberIdSecret = false, Action authCallback = null)
		{
			if (!_authInProgress)
			{
				_lastAuth = () => { AuthGuest(true); };
				_doNotRememberIdSecret = doNotRememberIdSecret;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var route = _url + "api/v0/accounts.getAccount";
				var request = new Request(RequestMethod.POST, route);

				_networkManager.Reset();
				_networkManager.SendRequest(request.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth in progress");
			}
		}

		public void AuthWebVk(string vkId, string vkSecret, Action authCallback = null)
		{
			if (_platform != Platform.webvk)
			{
				throw new Exception("Wrong platform");
			}
			else if (!_authDone && !_authInProgress)
			{
				_vkId = vkId;
				_vkSecret = vkSecret;
				_vkToken = "";
				_lastAuth = () => { AuthWebVk(_vkId, _vkSecret); };
				_doNotRememberIdSecret = true;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var route = _url + string.Format("api/v0/accounts.getAccount?vksecret={0}&vkid={1}", vkSecret, vkId);
				var authRequest = new Request(RequestMethod.POST, route);

				_networkManager.SendRequest(authRequest.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth already done");
			}
		}

		public void AuthVkSdk(string vkToken, Action authCallback = null)
		{
			if (_platform != Platform.android && _platform != Platform.ios && _platform != Platform.stdl)
			{
				throw new Exception("Wrong platform");
			}
			else if (!_authDone && !_authInProgress)
			{
				_vkId = "";
				_vkSecret = "";
				_vkToken = vkToken;
				_lastAuth = () => { AuthVkSdk(_vkToken); };
				_doNotRememberIdSecret = true;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var route = _url + string.Format("api/v0/accounts.getAccount?vktoken={0}", vkToken);
				var authRequest = new Request(RequestMethod.POST, route);

				_networkManager.SendRequest(authRequest.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth already done");
			}
		}

		public void AuthWebOdnoklassniki(string okSecret, string okId, string okSessionKey, Action authCallback = null)
		{
			if (_platform != Platform.webok)
			{
				throw new Exception("Wrong platform");
			}
			else if (!_authDone && !_authInProgress)
			{
				_okSecret = okSecret;
				_okId = okId;
				_okSessionKey = okSessionKey;
				_lastAuth = () => { AuthWebOdnoklassniki(_okSecret, _okId, _okSessionKey); };
				_doNotRememberIdSecret = true;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var route = _url + string.Format("api/v0/accounts.getAccount?oksecret={0}&okid={1}&oksessionkey={2}", okSecret, okId, okSessionKey);
				var authRequest = new Request(RequestMethod.POST, route);

				_networkManager.SendRequest(authRequest.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth already done");
			}
		}

		public void AuthOdnoklassnikiSdk(string okToken, Action authCallback = null)
		{
			if (_platform != Platform.android && _platform != Platform.ios && _platform != Platform.stdl)
			{
				throw new Exception("Wrong platform");
			}
			else if (!_authDone && !_authInProgress)
			{
				_okSecret = "";
				_okId = "";
				_okSessionKey = okToken;
				_lastAuth = () => { AuthOdnoklassnikiSdk(_okSessionKey); };
				_doNotRememberIdSecret = true;
				_authCallback = authCallback;
				_authInProgress = true;
				_authDone = false;

				var route = _url + string.Format("api/v0/accounts.getAccount?oksessionkey={0}", okToken);
				var authRequest = new Request(RequestMethod.POST, route);

				_networkManager.SendRequest(authRequest.www, AuthCallback);
			}
			else
			{
				throw new Exception("Auth already done");
			}
		}

		public object GetClientIdAndSecret()
		{
			return _account != null ? _account.IdAndSecret() : null;
		}

		private void FlushProfile(string data)
		{
			var route = _url + "api/v0/profile.updateProfile";
			var request = new Request(RequestMethod.POST, route, data);
			
			_networkManager.SendRequest(request.www, (error, response) =>
			{
				if (error != null)
				{
					Print(error);
					throw new Exception(error.message);
				}
				else
				{
					Print(response);
				}
			});
		}

		private void GetProfile()
		{
			var route = _url + (_account.prof ? "api/v0/profile.getProfile" : "api/v0/profile.createProfile");
			var request = new Request(RequestMethod.GET, route);
			
			_networkManager.SendRequest(request.www, (error, response) =>
			{
				if (error != null)
				{
					Print(error);
					_authInProgress = false;
					_authDone = false;
					throw new Exception(error.message);
				}
				else
				{
					Print(response);
					_profile = new Profile(response);
					_profile.FlushCallback = FlushProfile;
					_authInProgress = false;
					_authDone = true;
					if (_authCallback != null)
					{
						_authCallback();
						_authCallback = null;
					}
					if (_reauthCallback != null)
					{
						_reauthCallback();
						_reauthCallback = null;
					}
				}
			});
		}

		public void PostRecord(int value, string segment = "def")
		{
			var route = _url + string.Format("api/v0/tops.postARecord?value={0}&segment={1}", value, segment);
			var request = new Request(RequestMethod.POST, route);
			
			_networkManager.SendRequest(request.www, (error, response) =>
			{
				if (error != null)
				{
					Print(error);
					throw new Exception(error.message);
				}
				else
				{
					Print(response);
				}
			});
		}

		public void GetRecord(string segment = "def", Action<Record> callback = null)
		{
			var route = _url + string.Format("api/v0/tops.getPlayerRecord?segment={0}", segment);
			var request = new Request(RequestMethod.GET, route);
			
			_networkManager.SendRequest(request.www, (error, response) =>
			{
				if (error != null)
				{
					Print(error);
					throw new Exception(error.message);
				}
				else
				{
					Print(response);
					if (callback != null)
					{
						callback(new Record(response));
					}
				}
			});
		}

		public void GetLeaders(string segment = "def", Action<Leaderboard> callback = null)
		{
			var route = _url + string.Format("api/v0/tops.getLeadersOverall?segment={0}", segment);
			var request = new Request(RequestMethod.GET, route);
			
			_networkManager.SendRequest(request.www, (error, response) =>
			{
				if (error != null)
				{
					Print(error);
					throw new Exception(error.message);
				}
				else
				{
					Print(response);
					if (callback != null)
					{
						callback(new Leaderboard(response));
					}
				}
			});
		}

		public void Quit()
		{
			_networkManager.Reset();
			_account = null;
			_profile = null;
			_authInProgress = false;
			_authDone = false;
			_reauthCallback = null;
			_authCallback = null;
		}

		public void Reauth(Action reauthCallback)
		{
			Quit();
			_reauthCallback = reauthCallback;

			if (AutoReauth)
			{
				if (_lastAuth != null)
				{
					_lastAuth();
				}
				else
				{
					throw new Exception("No previous auth");
				}
			}
			else
			{
				throw new Exception("Session is dead, reauth please");
			}
		}

		public static void Print(object log)
		{
			if (_instance != null && _instance._enableLogs)
			{
				Debug.Log(log);
			}
		}

		public void Test()
		{
#if UNITY_EDITOR
			var route = (_localhost ? "http://localhost:8000/" : string.Format("https://{0}-{1}.{2}/", _projectName, _environment, _domainName)) + 
			            "/api/v0/dNxX6GsKmrbKfhfg5gNARgrhbvpBZZanJKrwhJqDT96Z7GVLkH6dKqRq96YC7Tgr9wYdPeqd/info";
			var request = new Request(RequestMethod.GET, route);
			var tempNetworkManager = new GameObject("tempNetworkManager");
			_networkManager = tempNetworkManager.AddComponent<NetworkManager>();		
			_networkManager.SendTestRequest(request.www, (error, response) =>
			{
				_networkManager = null;
				DestroyImmediate(tempNetworkManager);
				if (error != null)
				{
					EditorUtility.DisplayDialog("Test request failed", error.index + " " + error.message, "OK");
				}
				else
				{
					EditorUtility.DisplayDialog("Test request success", "Success", "OK");
				}
			});
#endif
		}
	}
}