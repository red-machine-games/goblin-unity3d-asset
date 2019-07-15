using UnityEngine;

namespace Gbase
{
    public class Record
    {
        public int rec;

        public Record(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }
    }
}