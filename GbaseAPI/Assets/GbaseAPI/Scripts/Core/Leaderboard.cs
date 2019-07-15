using System;
using UnityEngine;

namespace Gbase
{
    [Serializable]
    public class LeaderboardRecord
    {
        public int score;
        public int hid;
        public string pdata;

        private object _pData;

        public T GetProfileData<T>()
        {
            if (_pData == null && pdata.Length > 0)
            {
                _pData = JsonUtility.FromJson<T>(pdata);
            }
            else
            {
                _pData = default(T);
            }
            return (T) _pData;
        }
    }
    
    public class Leaderboard
    {
        public LeaderboardRecord[] records;

        public Leaderboard(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
            foreach (var record in records)
            {
                var p = json.IndexOf("\"pdata\":{", StringComparison.Ordinal);
                var pp = json.IndexOf(",{\"score\":", StringComparison.Ordinal);
                
                if (p > 0)
                {
                    if (pp < 0)
                    {
                        pp = json.IndexOf("],\"len\":", StringComparison.Ordinal);
                    }
                    record.pdata = json.Substring(p, pp - p - 1).Substring(8);
                }
            }
        }
    }
}