#Examples

1) First time auth user and save user name to profile
```csharp
// GbaseAPIManager.cs
public static void Auth(Action callback = null)
{
	GbaseAPI.Instance.AuthGuest(false, callback);
}
public static void SetProfile<T>(<T> profile)
{
	GbaseAPI.Instance.Profile.SetProfileData(profile);
}
public static string GetName()
{
	return GbaseAPI.Instance.Profile.GetProfileData().GetString("name"));
}
public static void SaveProfile()
{
	GbaseAPI.Instance.Profile.FlushData();
}

// Profile.cs
int level;
string name;

// AnotherClass.cs
...
GbaseAPIManager.Auth();
...
// get user name
GbaseAPIManager.SetProfile(new Profile());
GbaseAPIManager.SaveProfile();
...
```

2) Check profile version. If version is low then increment it and update profile
```csharp
// MyClass.cs
public int level = 1;
public bool isMale = true;
public string[] slots = {"head", "body", "legs", "weapon"};

// GbaseAPIManager.cs
public static int GetVersion()
{
	return GbaseAPI.Instance.Profile.GetVer();
}
public static void SetVersion(int v)
{
	return GbaseAPI.Instance.Profile.SetVer(v);
}
public static void SetProfile<T>(<T> profile)
{
	GbaseAPI.Instance.Profile.SetProfileData(profile);
}
public static void SaveProfile()
{
	GbaseAPI.Instance.Profile.FlushData();
}

// NewProfile.cs
int level;
string name;
bool newField;

// AnotherClass.cs
...
// auth already done
var currentVersion = 2;
if (GbaseAPIManager.GetVersion() < currentVersion)
{
	GbaseAPIManager.SetVersion(currentVersion);
	GbaseAPIManager.SetProfile(new NewProfile());
	GbaseAPIManager.SaveProfile();
}
...
```

3) If user decided to auth with Odnoklassniki API, but has progress at old profile. Get progress from old profile and save it to new profile
```csharp
// MyClass.cs
public int coins;
public int level;
public string[] achievements;

// GbaseAPIManager.cs
public static void Auth(Action callback = null)
{
	GbaseAPI.Instance.AuthGuest(false, callback);
}
public static void AuthOdnoklassnikiSdk(string okToken, Action callback = null)
{
	GbaseAPI.Instance.AuthOdnoklassnikiSdk(okToken, callback);
}
public static void SetProfile<T>(<T> profile)
{
	GbaseAPI.Instance.Profile.SetProfileData(profile);
}
public static Progress GetProgress()
{
	return GbaseAPI.Instance.Profile.GetProfileData<Progress>());
}
public static void SaveProfile()
{
	GbaseAPI.Instance.Profile.FlushData();
}
public static void Quit()
{
	GbaseAPI.Instance.Quit();
}

// Progress.cs
int progress;

// AnotherClass.cs
...
GbaseAPIManager.Auth();
...
// get user decision to use Odnoklassniki API
var progress = GbaseAPIManager.GetProgress();
GbaseAPIManager.Quit();
GbaseAPIManager.AuthOdnoklassnikiSdk(okToken, () => 
{
	// inside callback, to make sure auth is done
	GbaseAPIManager.SetProfile(new Progress());
	GbaseAPIManager.SaveProfile();
});
...
```
4) Example of calling of `GetLeaders()` function. Callback print amount of records and first leader name
```csharp
// GbaseAPIManager.cs
public static void GetLeaders(string segment, Action<Leaderboard> callback = null)
{
	GbaseAPI.Instance.GetLeaders(segment, callback);
}

// PublicProfileData.cs
string name;

// SomeClass.cs
...
// auth already done
GbaseAPIManager.GetLeaders("mySegment", l =>
{
    print(l.records.Length);
    var pdata = l.records[0].GetProfileData<TestClass>();
	print(pdata != null ? pdata.name : "pdata is null");
});
...
```