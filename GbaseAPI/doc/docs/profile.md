#Profile

After successful authentication you can call `GbaseAPI.Instance.Profile` to get access to user profile.  
Profile method:

* **`int` GetHumanId()** - returns readable id of user's profile.
* **`string or null` GetOkId()** - returns id received from Odnoklassniki API and stored in profile, after authenticating via **AuthOdnoklassniki** method.
* **`int` GetVer()** - returns version of user's profile
* **`void` SetVer(int v)** - sets version of user's profile
* **`T` GetProfileData<T\>()** - returns object of class `T` representing private profile data.
* **`void` SetProfileData<T\>(T data)** - rewrite local private profile data. To persist changes to server you should call `FlushData()` method;
* **`T` GetPublicProfileData<T\>()** - returns object of class `T` representing public profile data.
* **`void` SetProfileData<T\>(T data)** - rewrite local public profile data. To persist changes to server you should call `FlushData()` method;
* **`void` FlushData()** - send all profile changes to the server

##Next step

[Records and leaderboards page](/records)  
 
See also [examples page](/examples)