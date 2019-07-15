using UnityEngine;

namespace Gbase
{
    public class GbaseError
    {
        public int index;
        public string message;

        public GbaseError(int index, string message)
        {
            this.index = index;
            this.message = message;
        }
        
        public GbaseError(string json)
        {
            JsonUtility.FromJsonOverwrite(json, this);
        }

        public override string ToString()
        {
            return "index: " + index + "\nmessage: " + message;
        }
    }
}