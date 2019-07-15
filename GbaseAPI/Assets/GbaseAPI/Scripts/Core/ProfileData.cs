using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gbase
{
    [Serializable]
    public class ProfileData
    {
        public string[] Fields;

        private Dictionary<string, string> _fields = new Dictionary<string, string>();

        public void Init()
        {
            _fields.Clear();
            if (Fields == null)
            {
                Fields = new string[0];
                return;
            }
            
            foreach (var pair in Fields)
            {
                var p = pair.Split(':');

                if (p.Length == 2)
                {
                    _fields.Add(p[0], p[1]);
                }
                else if (p.Length > 2)
                {
                    var key = p[0];
                    p[0] = "";
                    _fields.Add(key, string.Join(":", p).Substring(1));
                }
            }
        }

        public void PrepareToFlush()
        {
            Fields = new string[_fields.Count];

            var i = 0;
            foreach (var pair in _fields)
            {
                Fields[i++] = pair.Key + ':' + pair.Value;
            }
        }
        
        public void SetValue<T> (string key, T value)
        {
            var t = typeof(T);
            string val;
            
            if (t.IsPrimitive || t.Name == "String")
            {
                val = value.ToString();
            }
            else
            {
                val = JsonUtility.ToJson(value);
            }
            
            if (ContainsKey(key))
            {
                _fields[key] = val;
            }
            else
            {
                _fields.Add(key, val);
            }
        }

        public int GetInt(string key)
        {
            if (!ContainsKey(key))
            {
                Debug.LogWarning("Profile don't has key " + key + " of type " + typeof(int).Name);
                return 0;
            }

            return int.Parse(_fields[key]);
        }

        public float GetFloat(string key)
        {
            if (!ContainsKey(key))
            {
                Debug.LogWarning("Profile don't has key " + key + " of type " + typeof(float).Name);
                return 0;
            }

            return float.Parse(_fields[key]);
        }

        public bool GetBool(string key)
        {
            if (!ContainsKey(key))
            {
                Debug.LogWarning("Profile don't has key " + key + " of type " + typeof(bool).Name);
                return false;
            }

            return bool.Parse(_fields[key]);
        }

        public string GetString(string key)
        {
            if (!ContainsKey(key))
            {
                Debug.LogWarning("Profile don't has key " + key + " of type " + typeof(string).Name);
                return "";
            }

            return _fields[key];
        }

        public T GetValue<T>(string key)
        {
            var t = typeof(T);

            if (t.IsPrimitive || t.Name == "String")
            {
                throw new Exception("Primitives and strings not supported. Use other methods");
            }
            if (!ContainsKey(key))
            {
                Debug.LogWarning("Profile don't has key " + key + " of type " + t.Name);
                return default(T);
            }

            return JsonUtility.FromJson<T>(_fields[key]);
        }

        private bool ContainsKey(string key)
        {
            return _fields.ContainsKey(key);
        }
    }
}