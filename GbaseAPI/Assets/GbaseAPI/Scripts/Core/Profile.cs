using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gbase
{
    public class Profile
    {
        public int humanId;
        public string ok;
        public string vk;
        public int ver;
        public string profileData;
        public string publicProfileData;
        public Action<string> FlushCallback;

        private object _pData;
        private object _ppData;
        private bool _versionChanged;
        private bool _profileDataChanged;
        private bool _publicProfileDataChanged;
        
        public Profile(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
            _pData = null;
            _ppData = null;
            profileData = "";
            publicProfileData = "";

            var p = json.IndexOf("\"profileData\":{", StringComparison.Ordinal);
            var pp = json.IndexOf("\"publicProfileData\":{", StringComparison.Ordinal);

            if (p < pp)
            {
                if (pp > 0)
                {
                    publicProfileData = json.Substring(pp, json.Length - pp - 1).Substring(20);
                }
                if (p > 0)
                {
                    if (pp < 0)
                    {
                        pp = json.Length;
                    }
                    profileData = json.Substring(p, pp - p - 1).Substring(14);
                }
            }
            else
            {
                if (p > 0)
                {
                    profileData = json.Substring(p, json.Length - p - 1).Substring(14);
                }
                if (pp > 0)
                {
                    if (p < 0)
                    {
                        p = json.Length;
                    }
                    
                    publicProfileData = json.Substring(pp, p - pp - 1).Substring(20);
                }
            }
        }
        
        public int GetHumanId()
        {
            return humanId;
        }

        public string GetOkId()
        {
            return ok;
        }

        public int GetVer()
        {
            return ver;
        }

        public void SetVer(int v)
        {
            _versionChanged = true;
            ver = v;
        }

        public T GetProfileData<T>()
        {
            if (_pData == null)
            {
                if (profileData.Length > 0)
                {
                    _pData = JsonUtility.FromJson<T>(profileData);
                }
                else
                {
                    _pData = default(T);
                }
            }
            return (T) _pData;
        }
        
        public void SetProfileData<T>(T data)
        {
            _profileDataChanged = true;
            _pData = data;
        }

        public T GetPublicProfileData<T>()
        {
            if (_ppData == null)
            {
                if (publicProfileData.Length > 0)
                {
                    _ppData = JsonUtility.FromJson<T>(publicProfileData);
                }
                else
                {
                    _ppData = default(T);
                }
            }

            return (T) _ppData;
        }
        
        public void SetPublicProfileData<T>(T data)
        {
            _publicProfileDataChanged = true;
            _ppData = data;
        }

        public void FlushData()
        {
            if (FlushCallback != null)
            {
                FlushCallback(GetFlushData());
            }
        }

        private string GetFlushData()
        {
            var flushData = new List<string>();

            if (_versionChanged)
            {
                flushData.Add("\"ver\":" + ver);
            }
            if (_profileDataChanged)
            {
                flushData.Add("\"profileData\":" + JsonUtility.ToJson(_pData));
            }
            if (_publicProfileDataChanged)
            {
                flushData.Add("\"publicProfileData\":" + JsonUtility.ToJson(_ppData));
            }

            _versionChanged = false;
            _profileDataChanged = false;
            _publicProfileDataChanged = false;
            
            return WWW.UnEscapeURL('{' + string.Join(",", flushData.ToArray()) + '}');
        }
    }
}