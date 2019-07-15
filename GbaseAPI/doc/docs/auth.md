#Authentication

GbaseApi has 4 authentication methods:

* **AuthGuest(bool doNotRemeberIdSecret, Action callback)** - basic authorization. If it's first authorization of a user, method will create new account and profile for user and if `doNotRemeberIdSecret == false` save account id and secret to PlayerPrefs. If user already has account id and secret, method will authorize user with this id and secret and get existing account and profile. Method only do authorization once per session, and wont reauthorize user after first call of any method: **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.
* **AuthGuestCustom(string gCustomId, string gCsutomSecret, bool createIfNotExisting, bool doNotRemeberIdSecret, Action callback)** - same as previous method, but uses custom id and secret params instead of already existing native id and secret. If `createIfNotExisting` flag is true method will create new account and profile for user if it doesn't exist. If `doNotRemeberIdSecret == false` save account id and secret to PlayerPrefs. If user already has account id and secret, method will authorize user with this id and secret and get existing account and profile. Method only do authorization once per session, and wont reauthorize user after first call of any method **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.
* **AuthGuestNew(bool doNotRemeberIdSecret, Action callback)** - it is the same authorization method as **AuthGuest**, but it always create new account and profile. Parameter `doNotRemeberIdSecret` works the same way.
* **AuthWebVk(string vkId, string vkSecret, Action callback)** - authorization with data received from VK API. Method works only with `webvk` platform and don't override previous account id and secret if they exist in PlayerPrefs. Method only do authorization once per session, and wont reauthorize user after first call of any method: **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.
* **AuthVkSdk(string vkToken, Action callback)** - authorization with token received from VK SDK. Method works only with `ios`, `android` and `stdl` platform and don't override previous account id and secret if they exist in PlayerPrefs. Method only do authorization once per session, and wont reauthorize user after first call of any method: **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.
* **AuthWebOdnoklassniki(string okSecret, string okId, string okSessionKey, Action callback)** - authorization with data received from Odnoklassniki API. Method works only with `webok` platform and don't override previous account id and secret if they exist in PlayerPrefs. Method only do authorization once per session, and wont reauthorize user after first call of any method: **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.
* **AuthOdnoklassnikiSdk(string okToken, Action callback)** - authorization with token received from Odnoklassniki SDK. Method works only with `ios`, `android` and `stdl` platform and don't override previous account id and secret if they exist in PlayerPrefs. Method only do authorization once per session, and wont reauthorize user after first call of any method: **AuthGuest**, **AuthGuestCustom**, **AuthWebVk**, **AuthVkSdk**, **AuthWebOdnoklassniki** or **AuthOdnoklassnikiSdk**.

All authenticate methods rewrite temporary account and profile data. Callback action is called when authentication is done.

GbaseApi can reauth by itself if session is dead and public flag `AutoReauth` is true. You can set it in editor or while runtime via scripts.

Method **GetClientIdAndSecret()** returns object of type `GClientData` with two variables `GClientId` and `GClientSecret`.

Method **Quit()** resets current session and deletes info about loaded account and profile. After calling this method you can authenticate again.

##Next step

[Profile page](/profile)  

See also [examples page](/examples)