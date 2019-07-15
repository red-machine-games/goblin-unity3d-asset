using System;
using System.Collections.Generic;
using UnityEngine;

namespace Gbase
{
    public class TestGuiScript : MonoBehaviour
    {
        public bool ShowLog;
        
        private string _customId = "id", _customSecret = "secret";
        private bool _newCustomAccount;
        
        private int _record;
        private string _segment = "def";
        private static List<string> _log = new List<string>();

        public static string Log
        {
            get
            {
                var l = "";
                foreach (var row in _log)
                {
                    l += row + '\n';
                }

                return l;
            }
            set
            {
                if (_log.Count > 6)
                {
                    _log.RemoveAt(0);
                }
                
                _log.Add(DateTime.Now.ToLongTimeString() + ' ' + (value.Length > 75 ? value.Substring(0, 75) : value));
            }
        }
        
        private void OnGUI()
        {
            if (GUI.Button(new Rect(5, 5, 150, 50), "Auth"))
            {
                GbaseAPI.Instance.AuthGuest();
            }
            if (GUI.Button(new Rect(160, 5, 150, 50), "Auth\nNO SAVE"))
            {
                GbaseAPI.Instance.AuthGuest(true);
            }
            
            if (GUI.Button(new Rect(5, 60, 150, 50), "Auth new guest"))
            {
                GbaseAPI.Instance.AuthGuestNew();
            }
            if (GUI.Button(new Rect(160, 60, 150, 50), "Auth new guest\nNO SAVE"))
            {
                GbaseAPI.Instance.AuthGuestNew(true);
            }
            
            if (GUI.Button(new Rect(5, 115, 150, 50), "Auth odnoklassniki\nwebok"))
            {
                GbaseAPI.Instance.AuthWebOdnoklassniki("secret", "123", "sessionkey");
            }
            if (GUI.Button(new Rect(160, 115, 150, 50), "Auth odnoklassniki\nonly token"))
            {
                GbaseAPI.Instance.AuthOdnoklassnikiSdk("secretToken");
            }
            
            if (GUI.Button(new Rect(5, 170, 150, 50), "Auth vk\nwebok"))
            {
                GbaseAPI.Instance.AuthWebVk("123", "secret");
            }
            if (GUI.Button(new Rect(160, 170, 150, 50), "Auth vk\nonly token"))
            {
                GbaseAPI.Instance.AuthVkSdk("vkToken");
            }
            
            GUI.Label(new Rect(5, 225, 100, 25), "CustomId");
            _customId = GUI.TextField(new Rect(105, 225, 50, 25), _customId);
            GUI.Label(new Rect(5, 250, 100, 25), "CustomSecret");
            _customSecret = GUI.TextField(new Rect(105, 250, 50, 25), _customSecret);
            
            if (GUI.Button(new Rect(160, 225, 150, 50), "Register\nnew custom"))
            {
                GbaseAPI.Instance.AuthGuestCustom(_customId, _customSecret, true);
            }
            if (GUI.Button(new Rect(315, 225, 150, 50), "Login\ncustom"))
            {
                GbaseAPI.Instance.AuthGuestCustom(_customId, _customSecret, false);
            }

            GUI.Label(new Rect(5, 280, 100, 25), "Record value");
            var record = GUI.TextField(new Rect(105, 280, 50, 25), _record.ToString());
            try
            {
                _record = int.Parse(record);
            }
            catch { }
            GUI.Label(new Rect(5, 305, 100, 25), "Record segment");
            _segment = GUI.TextField(new Rect(105, 305, 50, 25), _segment);
            
            if (GUI.Button(new Rect(160, 280, 150, 50), "Post record"))
            {
                GbaseAPI.Instance.PostRecord(_record, _segment);
            }
            if (GUI.Button(new Rect(5, 335, 150, 50), "Get record"))
            {
                GbaseAPI.Instance.GetRecord(_segment, r =>
                {
                    print(r.rec);
                });
            }
            if (GUI.Button(new Rect(160, 335, 150, 50), "Get leaders"))
            {
                GbaseAPI.Instance.GetLeaders(_segment, l =>
                {
                    print(l.records.Length);
                    foreach (var r in l.records)
                    {
                        var rec = r.hid + "\n";
                        var pdata = r.GetProfileData<TestClass>();
                        if (pdata != null)
                        {
                            rec += pdata.a + "\n";
                            rec += pdata.c[1] + "\n";
                            rec += pdata.t.level + "\n";
                        }
                        else
                        {
                            rec += "pdata is null";
                        }

                        print(rec);
                    }
                });
            }

            
            if (GUI.Button(new Rect(Screen.width - 310, 5, 150, 50), "Get account"))
            {
                var idSecret = (GClientData) GbaseAPI.Instance.GetClientIdAndSecret();
                if (idSecret != null)
                {
                    print(idSecret.GClientId + " " + idSecret.GClientSecret);
                }
            }
            
            if (GUI.Button(new Rect(Screen.width - 155, 5, 150, 50), "Profile\nhumanId"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    print(GbaseAPI.Instance.Profile.GetHumanId());
                }
            }
            if (GUI.Button(new Rect(Screen.width - 155, 60, 150, 50), "Profile\nokId"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    print(GbaseAPI.Instance.Profile.GetOkId());
                }
            }
            if (GUI.Button(new Rect(Screen.width - 155, 115, 150, 50), "Profile\nver"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    print(GbaseAPI.Instance.Profile.GetVer());
                }
            }
            if (GUI.Button(new Rect(Screen.width - 155, 170, 150, 50), "Profile\nprofileData"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    var pdata = GbaseAPI.Instance.Profile.GetProfileData<TestClass>();
                    if (pdata != null)
                    {
                        print(pdata.a);
                        print(pdata.c[1]);
                        print(pdata.d[0]);
                        print(pdata.t.level);
                    }
                    else
                    {
                        print("profileData is null");
                    }
                }
            }
            if (GUI.Button(new Rect(Screen.width - 155, 225, 150, 50), "Profile\npublicProfileData"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    var pdata = GbaseAPI.Instance.Profile.GetPublicProfileData<TestClass>();
                    if (pdata != null)
                    {
                        print(pdata.a);
                        print(pdata.c[1]);
                        print(pdata.d[0]);
                        print(pdata.t.level);
                    }
                    else
                    {
                        print("publicProfileData is null");
                    }
                }
            }

            if (GUI.Button(new Rect(Screen.width - 155, 280, 150, 50), "Profile\nflush"))
            {
                if (GbaseAPI.Instance.Profile != null)
                {
                    GbaseAPI.Instance.Profile.SetProfileData(new TestClass());
                    GbaseAPI.Instance.Profile.SetPublicProfileData(new TestClass());
                    GbaseAPI.Instance.Profile.FlushData();
                }
            }

            if (ShowLog)
            {
                var style = GUI.skin.textArea;
                style.alignment = TextAnchor.LowerLeft;
                GUI.Label(new Rect(320, Screen.height - 180, Screen.width - 480, 170), Log, style);
            }

            if (GUI.Button(new Rect(Screen.width - 155, Screen.height - 55, 150, 50), "Quit"))
            {
                GbaseAPI.Instance.Quit();
            }
        }
    }
}