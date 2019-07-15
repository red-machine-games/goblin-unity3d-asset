#Records and leaderboards

Asset allow you to persist important values as "records". Just choose segment and save record or get top records of that segment.

Records method:

* **`void` PostRecord(int value, string segment)** - save given value as record of current segment (default segment is 'def')
* **`void` GetRecord(string segment, Action<Record> callback)** - get player record for current segment (default segment is 'def'). `Record` parameter of callback described [below](#Record)
* **`void` GetLeaders(string segment, Action<Leaderboard> callback)** - get leaderboard of records for current segment (default segment is 'def'). `Leaderboard` parameter of callback described [below](#Leaderboard)

##<a name="Record"></a>Record class

Simple class with single field:

* **`int` rec** - record value

##<a name="Leaderboard"></a>Leaderboard class

Class contains array of records with fields:

* **`int` score** - record value
* **`int` hid** - readable player id
* **`T` GetProfileData<T\>()** - return public profile data

##Next step  
 
See also [examples page](/examples)