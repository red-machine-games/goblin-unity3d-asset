using UnityEngine;

namespace Gbase
{
    public class GClientData
    {
        public readonly string GClientId;
        public readonly string GClientSecret;

        public GClientData(Account account)
        {
            GClientId = account.gClientId;
            GClientSecret = account.gClientSecret;
        }
    }
    
    public class Account
    {
        public string gClientId;
        public string gClientSecret;
        public string unicorn;
        public bool prof;

        public Account()
        {
            gClientId = "";
            gClientSecret = "";
            unicorn = "";
            prof = false;
        }
        
        public Account(string gClientId, string gClientSecret)
        {
            this.gClientId = gClientId;
            this.gClientSecret = gClientSecret;
            unicorn = "";
            prof = false;
        }
        
        public Account(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public GClientData IdAndSecret()
        {
            return new GClientData(this);
        }

        public void Save(string prefix)
        {
            PlayerPrefs.SetString(prefix + "gClientId", gClientId);
            PlayerPrefs.SetString(prefix + "gClientSecret", gClientSecret);
        }

        public static Account GetSavedAccount(string prefix)
        {
            if (PlayerPrefs.HasKey(prefix + "gClientId"))
            {
                return new Account(PlayerPrefs.GetString(prefix + "gClientId"), PlayerPrefs.GetString(prefix + "gClientSecret"));
            }
            return new Account();
        }
    }
}